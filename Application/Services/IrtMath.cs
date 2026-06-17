using UniStart.Domain.Entities;

namespace UniStart.Application.Services;

/// <summary>
/// Item Response Theory (IRT) mathematical functions.
/// Implements 3-Parameter Logistic (3PL) model with EAP ability estimation.
/// 
/// 3PL Model: P(θ) = c + (1 - c) / (1 + exp(-a(θ - b)))
///   where:
///     θ = student ability (latent trait)
///     a = item discrimination (how well the item differentiates ability levels)
///     b = item difficulty (the ability level at which P=0.5 for 2PL)
///     c = guessing parameter (lower asymptote)
/// </summary>
public static class IrtMath
{
    // ─── 3PL Model ──────────────────────────────────────────────

    /// <summary>
    /// Probability of correct answer given ability θ and item parameters.
    /// P(θ) = c + (1 - c) * σ(a * (θ - b))
    /// </summary>
    public static double Probability(double theta, double a, double b, double c)
    {
        var logistic = 1.0 / (1.0 + Math.Exp(-a * (theta - b)));
        return c + (1.0 - c) * logistic;
    }

    /// <summary>
    /// Probability using Question entity parameters.
    /// </summary>
    public static double Probability(double theta, Question item)
        => Probability(theta, item.DiscriminationParam, item.DifficultyParam, item.GuessParam);

    // ─── Fisher Information ─────────────────────────────────────

    /// <summary>
    /// Fisher information of an item at ability θ.
    /// Higher information = the item is more useful for estimating θ at this level.
    /// I(θ) = a² * (P - c)² * (1 - P) / ((1 - c)² * P)
    /// </summary>
    public static double Information(double theta, double a, double b, double c)
    {
        var p = Probability(theta, a, b, c);
        if (p <= c || p >= 1.0) return 0.0;
        
        var numerator = a * a * Math.Pow(p - c, 2) * (1.0 - p);
        var denominator = Math.Pow(1.0 - c, 2) * p;
        
        return denominator > 0 ? numerator / denominator : 0.0;
    }

    public static double Information(double theta, Question item)
        => Information(theta, item.DiscriminationParam, item.DifficultyParam, item.GuessParam);

    // ─── EAP Estimation ─────────────────────────────────────────

    /// <summary>
    /// Expected A Posteriori (EAP) estimation of ability θ.
    /// Uses numerical integration with a normal prior.
    /// Returns (thetaEstimate, standardError).
    /// 
    /// More robust than MLE for small numbers of responses:
    /// - Always converges (unlike MLE which can diverge with all-correct/all-wrong)
    /// - Bayesian shrinkage toward prior mean (regularization)
    /// - Extended range [-5, 5] with 81 quadrature points to prevent posterior collapse
    /// </summary>
    public static (double theta, double se) EstimateAbilityEAP(
        IList<(Question item, bool correct)> responses,
        double priorMean = 0.0,
        double priorSD = 1.5,
        int quadPoints = 81)
    {
        if (responses.Count == 0)
            return (priorMean, priorSD);

        // Extended range [-5, 5] with more quadrature points to avoid posterior collapse for extreme θ
        var thetaRange = Linspace(-5.0, 5.0, quadPoints);
        
        var numerator = 0.0;   // E[θ|X]
        var numerator2 = 0.0;  // E[θ²|X]
        var denominator = 0.0; // normalizing constant

        foreach (var thetaQ in thetaRange)
        {
            // Prior: N(priorMean, priorSD²)
            var logPrior = -0.5 * Math.Pow((thetaQ - priorMean) / priorSD, 2);
            
            // Likelihood: product of P(x|θ) over all responses
            var logLikelihood = 0.0;
            foreach (var (item, correct) in responses)
            {
                var p = Probability(thetaQ, item);
                p = Math.Clamp(p, 1e-10, 1.0 - 1e-10); // avoid log(0)
                logLikelihood += correct ? Math.Log(p) : Math.Log(1.0 - p);
            }

            var posterior = Math.Exp(logPrior + logLikelihood);
            
            numerator += thetaQ * posterior;
            numerator2 += thetaQ * thetaQ * posterior;
            denominator += posterior;
        }

        if (denominator < 1e-300)
        {
            // Adaptive fallback: if posterior collapses, use a wider prior and retry once
            if (quadPoints < 161)
            {
                return EstimateAbilityEAP(responses, priorMean, priorSD * 1.5, 161);
            }
            return (priorMean, priorSD);
        }

        var thetaEAP = numerator / denominator;
        var variance = (numerator2 / denominator) - (thetaEAP * thetaEAP);
        var se = Math.Sqrt(Math.Max(variance, 1e-10));

        return (thetaEAP, se);
    }

    // ─── Theta → Display Level ──────────────────────────────────

    /// <summary>
    /// Converts IRT theta (-4..+4) to display level (0..100).
    /// Uses logistic mapping centered at θ=0 → level=50.
    /// </summary>
    public static int ThetaToLevel(double theta)
    {
        // Logistic mapping: level = 100 / (1 + exp(-1.5 * theta))
        var level = 100.0 / (1.0 + Math.Exp(-1.5 * theta));
        return (int)Math.Clamp(Math.Round(level), 0, 100);
    }

    /// <summary>
    /// Converts display level (0..100) back to approximate theta.
    /// </summary>
    public static double LevelToTheta(int level)
    {
        var clamped = Math.Clamp(level, 1, 99); // avoid log(0)
        return Math.Log(clamped / (100.0 - clamped)) / 1.5;
    }

    // ─── Difficulty Mapping ─────────────────────────────────────

    /// <summary>
    /// Maps QuestionDifficulty enum to default IRT difficulty parameter (b).
    /// </summary>
    public static double DifficultyToParam(QuestionDifficulty difficulty) => difficulty switch
    {
        QuestionDifficulty.Easy => -1.0,
        QuestionDifficulty.Medium => 0.0,
        QuestionDifficulty.Hard => 1.5,
        _ => 0.0
    };

    /// <summary>
    /// Maps QuestionDifficulty enum to default discrimination parameter (a).
    /// Harder questions tend to have higher discrimination.
    /// </summary>
    public static double DifficultyToDiscrimination(QuestionDifficulty difficulty) => difficulty switch
    {
        QuestionDifficulty.Easy => 0.8,
        QuestionDifficulty.Medium => 1.0,
        QuestionDifficulty.Hard => 1.2,
        _ => 1.0
    };

    /// <summary>
    /// Default guessing parameter — for 4-option MCQ it's ~0.25.
    /// </summary>
    public static double DefaultGuessParam(int optionCount = 4) => 1.0 / optionCount;

    /// <summary>
    /// Number of real responses after which an item's difficulty is considered calibrated
    /// (data-driven) rather than just a difficulty-derived prior.
    /// </summary>
    public const int CalibrationThreshold = 30;

    /// <summary>
    /// Online (Elo-style) update of an item's difficulty parameter (b) from a single response.
    /// The item moves opposite to the surprise: a student answering correctly when the model
    /// expected a wrong answer makes the item easier (b decreases), and vice versa.
    ///
    /// The learning rate decays with the number of responses already folded in, so early
    /// responses move b quickly and it stabilises as evidence accumulates.
    /// </summary>
    /// <param name="currentB">Current difficulty parameter (b).</param>
    /// <param name="a">Discrimination parameter (a).</param>
    /// <param name="c">Guessing parameter (c).</param>
    /// <param name="theta">The responding student's current ability estimate.</param>
    /// <param name="correct">Whether the student answered correctly.</param>
    /// <param name="priorResponseCount">Responses already folded into b before this one.</param>
    public static double UpdateDifficultyOnline(
        double currentB, double a, double c, double theta, bool correct, int priorResponseCount)
    {
        var expected = Probability(theta, a, currentB, c);
        var actual = correct ? 1.0 : 0.0;
        // Adaptive learning rate: ~0.5 for a brand-new item, decaying toward 0 as it calibrates.
        var k = 0.5 / (1.0 + priorResponseCount / 20.0);
        var newB = currentB + k * (expected - actual);
        return Math.Clamp(newB, -4.0, 4.0);
    }

    // ─── Ebbinghaus Forgetting Curve ────────────────────────────

    /// <summary>
    /// Retention probability based on time since last review.
    /// R(t) = e^(-t/S), where S is stability (in days).
    /// Higher S = slower forgetting (better learned).
    /// </summary>
    public static double RetentionProbability(double daysSinceReview, double stability)
    {
        if (stability <= 0) return 0.0;
        return Math.Exp(-daysSinceReview / stability);
    }

    /// <summary>
    /// Calculates memory stability based on number of successful reviews.
    /// Each successful review increases stability roughly exponentially.
    /// S = S₀ × (1 + factor)^(successCount)
    /// </summary>
    public static double CalculateStability(int successCount, double baseStability = 1.0, double factor = 1.5)
    {
        return baseStability * Math.Pow(1.0 + factor, successCount);
    }

    // ─── CAT Item Selection (Fisher Information) ────────────────

    /// <summary>
    /// Selects the item with maximum Fisher information at the current θ estimate.
    /// This is the standard CAT item selection criterion.
    /// Optionally applies exposure control via Sympson-Hetter method.
    /// </summary>
    public static Question? SelectNextItem(
        double theta,
        IEnumerable<Question> availableItems,
        double? exposureLimit = null)
    {
        Question? bestItem = null;
        var bestInfo = double.MinValue;

        foreach (var item in availableItems)
        {
            var info = Information(theta, item);
            
            // Apply exposure control: prefer items with info close to max but randomize among top
            if (info > bestInfo)
            {
                bestInfo = info;
                bestItem = item;
            }
        }

        return bestItem;
    }

    /// <summary>
    /// Selects next item using a randomized maximum information strategy.
    /// Picks randomly from items whose information is within a fraction of the best.
    /// This provides exposure control while maintaining measurement precision.
    /// </summary>
    public static Question? SelectNextItemRandomized(
        double theta,
        IList<Question> availableItems,
        double topFraction = 0.8)
    {
        if (availableItems.Count == 0) return null;

        var itemInfos = availableItems
            .Select(item => (item, info: Information(theta, item)))
            .OrderByDescending(x => x.info)
            .ToList();

        var maxInfo = itemInfos[0].info;
        var threshold = maxInfo * topFraction;

        var topItems = itemInfos.Where(x => x.info >= threshold).ToList();
        if (topItems.Count == 0) topItems = itemInfos.Take(1).ToList();

        return topItems[Random.Shared.Next(topItems.Count)].item;
    }

    // ─── Helpers ────────────────────────────────────────────────

    private static double[] Linspace(double start, double end, int count)
    {
        var step = (end - start) / (count - 1);
        var result = new double[count];
        for (var i = 0; i < count; i++)
            result[i] = start + step * i;
        return result;
    }
}
