using UniStart.Domain.Entities;

namespace UniStart.Application.Services;

public static class IrtMath
{
    public static double Probability(double theta, double a, double b, double c)
    {
        var logistic = 1.0 / (1.0 + Math.Exp(-a * (theta - b)));
        return c + (1.0 - c) * logistic;
    }

    public static double Probability(double theta, Question item)
        => Probability(theta, item.DiscriminationParam, item.DifficultyParam, item.GuessParam);

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

    public static (double theta, double se) EstimateAbilityEAP(
        IList<(Question item, bool correct)> responses,
        double priorMean = 0.0,
        double priorSD = 1.5,
        int quadPoints = 81)
    {
        if (responses.Count == 0)
            return (priorMean, priorSD);

        var thetaRange = Linspace(-5.0, 5.0, quadPoints);
        
        var numerator = 0.0;
        var numerator2 = 0.0;
        var denominator = 0.0;

        foreach (var thetaQ in thetaRange)
        {
            var logPrior = -0.5 * Math.Pow((thetaQ - priorMean) / priorSD, 2);
            
            var logLikelihood = 0.0;
            foreach (var (item, correct) in responses)
            {
                var p = Probability(thetaQ, item);
                p = Math.Clamp(p, 1e-10, 1.0 - 1e-10);
                logLikelihood += correct ? Math.Log(p) : Math.Log(1.0 - p);
            }

            var posterior = Math.Exp(logPrior + logLikelihood);
            
            numerator += thetaQ * posterior;
            numerator2 += thetaQ * thetaQ * posterior;
            denominator += posterior;
        }

        if (denominator < 1e-300)
        {
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

    public static int ThetaToLevel(double theta)
    {
        var level = 100.0 / (1.0 + Math.Exp(-1.5 * theta));
        return (int)Math.Clamp(Math.Round(level), 0, 100);
    }

    public static double LevelToTheta(int level)
    {
        var clamped = Math.Clamp(level, 1, 99);
        return Math.Log(clamped / (100.0 - clamped)) / 1.5;
    }

    public static double DifficultyToParam(QuestionDifficulty difficulty) => difficulty switch
    {
        QuestionDifficulty.Easy => -1.0,
        QuestionDifficulty.Medium => 0.0,
        QuestionDifficulty.Hard => 1.5,
        _ => 0.0
    };

    public static double DifficultyToDiscrimination(QuestionDifficulty difficulty) => difficulty switch
    {
        QuestionDifficulty.Easy => 0.8,
        QuestionDifficulty.Medium => 1.0,
        QuestionDifficulty.Hard => 1.2,
        _ => 1.0
    };

    public static double DefaultGuessParam(int optionCount = 4) => 1.0 / optionCount;

    public const int CalibrationThreshold = 30;

    public static double UpdateDifficultyOnline(
        double currentB, double a, double c, double theta, bool correct, int priorResponseCount)
    {
        var expected = Probability(theta, a, currentB, c);
        var actual = correct ? 1.0 : 0.0;
        var k = 0.5 / (1.0 + priorResponseCount / 20.0);
        var newB = currentB + k * (expected - actual);
        return Math.Clamp(newB, -4.0, 4.0);
    }

    public static double RetentionProbability(double daysSinceReview, double stability)
    {
        if (stability <= 0) return 0.0;
        return Math.Exp(-daysSinceReview / stability);
    }

    public static double CalculateStability(int successCount, double baseStability = 1.0, double factor = 1.5)
    {
        return baseStability * Math.Pow(1.0 + factor, successCount);
    }

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
            
            if (info > bestInfo)
            {
                bestInfo = info;
                bestItem = item;
            }
        }

        return bestItem;
    }

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


    private static double[] Linspace(double start, double end, int count)
    {
        var step = (end - start) / (count - 1);
        var result = new double[count];
        for (var i = 0; i < count; i++)
            result[i] = start + step * i;
        return result;
    }
}
