using Microsoft.EntityFrameworkCore;
using UniStart.Domain.Entities;

namespace UniStart.Infrastructure.Data;

/// <summary>
/// Expansion batch 2: adds ~200 new CSCA Mathematics EN questions across all 20 chapters.
/// Uses a unique marker question to ensure idempotency.
/// </summary>
public class CscaMathEnExpansion2Seeder
{
    private readonly UniStartDbContext _context;

    public CscaMathEnExpansion2Seeder(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var mathSection = await _context.ExamSections
            .FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Mathematics") && !s.Name.Contains("(CN)"));
        if (mathSection == null) return;

        // Idempotency: skip if marker question already exists
        var marker = await _context.Questions
            .AnyAsync(q => q.Text == "[EXP2] If A = {1,2,3} and B = {2,3,4,5}, then A △ B (symmetric difference) =");
        if (marker) return;

        var topics = await _context.Topics
            .Where(t => t.SectionId == mathSection.Id)
            .ToDictionaryAsync(t => t.Name.Split(' ')[0], t => t.Id);

        int T(string prefix) => topics.TryGetValue(prefix, out var id) ? id : topics.Values.First();

        var questions = new List<Question>();

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 1: SETS (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("1.1.1"), "[EXP2] If A = {1,2,3} and B = {2,3,4,5}, then A △ B (symmetric difference) =",
            D.Medium, "A △ B = (A \\ B) ∪ (B \\ A) = {1} ∪ {4,5} = {1,4,5}.",
            "{1, 4, 5}", "{2, 3}", "{1, 2, 3, 4, 5}", "{4, 5}", 0.3));

        questions.Add(Q(T("1.1.2"), "[EXP2] Let U = ℝ, A = {x | x² - 4 ≤ 0}. Then ∁_U A =",
            D.Medium, "x² ≤ 4 means -2 ≤ x ≤ 2. Complement: (-∞,-2) ∪ (2,+∞).",
            "(-∞,-2) ∪ (2,+∞)", "[-2,2]", "(-2,2)", "{-2,2}", 0.2));

        questions.Add(Q(T("1.2.1"), "[EXP2] If A ⊂ B and B ⊂ A, then:",
            D.Easy, "A ⊂ B and B ⊂ A together imply A = B by definition of set equality.",
            "A = B", "A ∩ B = ∅", "A ∪ B = ∅", "A ≠ B", -1.0));

        questions.Add(Q(T("1.2.2"), "[EXP2] The power set P({a, b}) has how many elements?",
            D.Easy, "P({a,b}) = {∅, {a}, {b}, {a,b}}. That's 2² = 4 elements.",
            "4", "2", "3", "8", -0.8));

        questions.Add(Q(T("1.2.3"), "[EXP2] If A = {x | x² = 1}, then A has how many proper subsets?",
            D.Easy, "A = {-1, 1}. Proper subsets: ∅, {-1}, {1}. Count = 2² - 1 = 3.",
            "3", "2", "4", "1", -0.5));

        questions.Add(Q(T("1.4.1"), "[EXP2] If A = {1,2,3,4} and B = {3,4,5,6}, then A ∩ B =",
            D.Easy, "Common elements: {3, 4}.",
            "{3, 4}", "{1, 2, 3, 4, 5, 6}", "{1, 2}", "{5, 6}", -1.2));

        questions.Add(Q(T("1.4.2"), "[EXP2] If A ∪ B = A, which must be true?",
            D.Easy, "A ∪ B = A means every element of B is already in A, so B ⊆ A.",
            "B ⊆ A", "A ⊆ B", "A ∩ B = ∅", "A = ∅", -0.5));

        questions.Add(Q(T("1.4.3"), "[EXP2] De Morgan's law: ∁_U(A ∩ B) =",
            D.Medium, "De Morgan: complement of intersection = union of complements.",
            "(∁_U A) ∪ (∁_U B)", "(∁_U A) ∩ (∁_U B)", "∁_U(A ∪ B)", "A ∪ B", 0.0));

        questions.Add(Q(T("1.1.3"), "[EXP2] Which of the following is NOT a well-defined set?",
            D.Medium, "'All tall people' is subjective and not well-defined.",
            "The set of all tall people", "The set of all even numbers",
            "The set of all prime numbers less than 10", "The set of all vowels in English", 0.2));

        questions.Add(Q(T("1.4.4"), "[EXP2] If |A| = 5 and |B| = 3 and |A ∩ B| = 2, then |A ∪ B| =",
            D.Easy, "By inclusion-exclusion: |A ∪ B| = |A| + |B| - |A ∩ B| = 5 + 3 - 2 = 6.",
            "6", "8", "5", "10", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 2: INEQUALITIES (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("2.1.1"), "[EXP2] Solve: 3x - 7 > 2x + 1",
            D.Easy, "3x - 2x > 1 + 7 → x > 8.",
            "x > 8", "x > 6", "x < 8", "x > -8", -1.0));

        questions.Add(Q(T("2.1.2"), "[EXP2] Solve: |2x - 3| < 5",
            D.Medium, "-5 < 2x - 3 < 5 → -2 < 2x < 8 → -1 < x < 4.",
            "-1 < x < 4", "x < 4", "x > -1", "-4 < x < 1", 0.2));

        questions.Add(Q(T("2.1.3"), "[EXP2] If a > b > 0, which inequality is always true?",
            D.Medium, "If a > b > 0, then 1/a < 1/b (reciprocal reverses inequality for positives).",
            "1/a < 1/b", "a² < b²", "-a > -b", "a - b < 0", 0.3));

        questions.Add(Q(T("2.2.1"), "[EXP2] Solve: x² - 5x + 6 > 0",
            D.Medium, "(x-2)(x-3) > 0 → x < 2 or x > 3.",
            "x < 2 or x > 3", "2 < x < 3", "x > 3", "x < 2", 0.0));

        questions.Add(Q(T("2.2.2"), "[EXP2] Solve: x² + 4x + 4 ≤ 0",
            D.Medium, "(x+2)² ≤ 0. A square is ≥ 0, equals 0 only at x = -2.",
            "x = -2", "x ≤ -2", "No solution", "x ≤ 0", 0.4));

        questions.Add(Q(T("2.3.1"), "[EXP2] Find the min value of f(x) = x + 4/x for x > 0 using AM-GM:",
            D.Hard, "By AM-GM: x + 4/x ≥ 2√(x · 4/x) = 2√4 = 4. Equality when x = 4/x → x = 2.",
            "4", "2", "8", "2√2", 0.8));

        questions.Add(Q(T("2.3.2"), "[EXP2] The solution set of (x-1)/(x+2) ≥ 0 is:",
            D.Medium, "Sign analysis: positive when both factors same sign. x ≤ -2 (excluded) or x ≥ 1. Answer: (-∞,-2) ∪ [1,+∞).",
            "(-∞,-2) ∪ [1,+∞)", "[-2, 1]", "[1, +∞)", "(-∞,-2]", 0.5));

        questions.Add(Q(T("2.1.1"), "[EXP2] If -3 < x < 5, then |x| < ?",
            D.Easy, "The maximum of |x| in (-3,5) is 5 (at x approaching 5). So |x| < 5.",
            "5", "3", "8", "4", -0.3));

        questions.Add(Q(T("2.2.1"), "[EXP2] The discriminant of x² - 6x + 9 = 0 is:",
            D.Easy, "Δ = b² - 4ac = 36 - 36 = 0. One repeated root.",
            "0", "36", "-36", "72", -0.8));

        questions.Add(Q(T("2.3.1"), "[EXP2] For a, b > 0: (a+b)/2 ≥ √(ab) is called:",
            D.Easy, "This is the AM-GM inequality (Arithmetic Mean ≥ Geometric Mean).",
            "AM-GM inequality", "Cauchy-Schwarz inequality",
            "Triangle inequality", "Bernoulli inequality", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 3: FUNCTIONS (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("3.1.1"), "[EXP2] The domain of f(x) = √(3-x) is:",
            D.Easy, "3-x ≥ 0 → x ≤ 3. Domain: (-∞,3].",
            "(-∞, 3]", "[3, +∞)", "(-∞, 3)", "(3, +∞)", -0.8));

        questions.Add(Q(T("3.1.2"), "[EXP2] The range of f(x) = 1/(x²+1) is:",
            D.Medium, "x²+1 ≥ 1, so 0 < 1/(x²+1) ≤ 1. Range: (0,1].",
            "(0, 1]", "[0, 1]", "(0, +∞)", "[1, +∞)", 0.3));

        questions.Add(Q(T("3.2.1"), "[EXP2] If f(x) = f(-x) for all x, then f is:",
            D.Easy, "f(x) = f(-x) is the definition of an even function.",
            "Even", "Odd", "Neither", "Both", -1.0));

        questions.Add(Q(T("3.2.2"), "[EXP2] Is f(x) = x³ + x odd, even, or neither?",
            D.Easy, "f(-x) = -x³ - x = -(x³ + x) = -f(x). So f is odd.",
            "Odd", "Even", "Neither", "Both", -0.5));

        questions.Add(Q(T("3.3.1"), "[EXP2] If f(x) = 2^x, then f(3) - f(1) =",
            D.Easy, "f(3) = 8, f(1) = 2. Difference = 6.",
            "6", "4", "8", "2", -0.8));

        questions.Add(Q(T("3.3.2"), "[EXP2] log₂ 16 =",
            D.Easy, "2⁴ = 16, so log₂ 16 = 4.",
            "4", "8", "2", "16", -1.2));

        questions.Add(Q(T("3.4.1"), "[EXP2] The inverse of f(x) = 3x - 2 is:",
            D.Medium, "y = 3x - 2 → x = (y+2)/3 → f⁻¹(x) = (x+2)/3.",
            "(x + 2)/3", "(x - 2)/3", "3x + 2", "2 - 3x", 0.0));

        questions.Add(Q(T("3.4.2"), "[EXP2] The graph of y = f(x-2) is obtained from y = f(x) by:",
            D.Easy, "Replacing x with x-2 shifts the graph 2 units to the right.",
            "Shifting 2 units right", "Shifting 2 units left",
            "Shifting 2 units up", "Shifting 2 units down", -0.5));

        questions.Add(Q(T("3.1.1"), "[EXP2] The domain of f(x) = ln(x-1) + √(5-x) is:",
            D.Hard, "Need x-1 > 0 and 5-x ≥ 0 → x > 1 and x ≤ 5. Domain: (1, 5].",
            "(1, 5]", "[1, 5]", "(1, 5)", "[1, 5)", 0.6));

        questions.Add(Q(T("3.2.1"), "[EXP2] The period of f(x) = sin(2x) is:",
            D.Easy, "Period of sin(kx) = 2π/|k| = 2π/2 = π.",
            "π", "2π", "π/2", "4π", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 4: TRIGONOMETRIC FUNCTIONS (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("4.1.1"), "[EXP2] Convert 150° to radians:",
            D.Easy, "150° × π/180° = 5π/6.",
            "5π/6", "3π/4", "2π/3", "7π/6", -0.8));

        questions.Add(Q(T("4.1.2"), "[EXP2] sin(π/3) =",
            D.Easy, "sin 60° = √3/2.",
            "√3/2", "1/2", "√2/2", "1", -1.2));

        questions.Add(Q(T("4.2.1"), "[EXP2] cos(2π - θ) =",
            D.Easy, "cos(2π - θ) = cos θ (cosine is periodic with 2π, and cos(-θ) = cos θ).",
            "cos θ", "-cos θ", "sin θ", "-sin θ", -0.5));

        questions.Add(Q(T("4.2.2"), "[EXP2] sin²θ + cos²θ =",
            D.Easy, "Pythagorean identity: sin²θ + cos²θ = 1.",
            "1", "0", "sin 2θ", "2", -1.5));

        questions.Add(Q(T("4.3.1"), "[EXP2] The amplitude of y = 3sin(2x + π/4) is:",
            D.Easy, "Amplitude = |A| = 3.",
            "3", "2", "π/4", "6", -0.8));

        questions.Add(Q(T("4.3.2"), "[EXP2] The phase shift of y = sin(x - π/3) is:",
            D.Easy, "Phase shift = π/3 to the right.",
            "π/3 to the right", "π/3 to the left", "3 to the right", "No shift", -0.5));

        questions.Add(Q(T("4.1.1"), "[EXP2] tan(π/4) =",
            D.Easy, "tan 45° = sin 45°/cos 45° = 1.",
            "1", "0", "√3", "1/√3", -1.5));

        questions.Add(Q(T("4.2.1"), "[EXP2] In which quadrant is sin θ > 0 and cos θ < 0?",
            D.Easy, "Quadrant II: sin positive, cos negative.",
            "Quadrant II", "Quadrant I", "Quadrant III", "Quadrant IV", -0.8));

        questions.Add(Q(T("4.1.2"), "[EXP2] cos(3π/4) =",
            D.Easy, "3π/4 is in QII. cos(3π/4) = -cos(π/4) = -√2/2.",
            "-√2/2", "√2/2", "-1/2", "1/2", -0.5));

        questions.Add(Q(T("4.3.1"), "[EXP2] The period of y = tan(x/2) is:",
            D.Medium, "Period of tan(kx) = π/|k| = π/(1/2) = 2π.",
            "2π", "π", "π/2", "4π", 0.2));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 5: INVERSE TRIG FUNCTIONS (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("5.1.1"), "[EXP2] arcsin(1/2) =",
            D.Easy, "sin(π/6) = 1/2, so arcsin(1/2) = π/6.",
            "π/6", "π/3", "π/4", "π/2", -1.0));

        questions.Add(Q(T("5.1.2"), "[EXP2] arccos(0) =",
            D.Easy, "cos(π/2) = 0, so arccos(0) = π/2.",
            "π/2", "0", "π", "π/4", -0.8));

        questions.Add(Q(T("5.2.1"), "[EXP2] arctan(1) =",
            D.Easy, "tan(π/4) = 1, so arctan(1) = π/4.",
            "π/4", "π/2", "π/3", "π/6", -1.0));

        questions.Add(Q(T("5.2.2"), "[EXP2] The range of arcsin(x) is:",
            D.Easy, "arcsin: [-1,1] → [-π/2, π/2].",
            "[-π/2, π/2]", "[0, π]", "(-π, π)", "[0, 2π]", -0.5));

        questions.Add(Q(T("5.1.1"), "[EXP2] arcsin(-√3/2) =",
            D.Medium, "sin(-π/3) = -√3/2, so arcsin(-√3/2) = -π/3.",
            "-π/3", "π/3", "-2π/3", "2π/3", 0.0));

        questions.Add(Q(T("5.1.2"), "[EXP2] arccos(-1) =",
            D.Easy, "cos(π) = -1, so arccos(-1) = π.",
            "π", "0", "-π", "2π", -0.8));

        questions.Add(Q(T("5.2.1"), "[EXP2] arctan(√3) =",
            D.Easy, "tan(π/3) = √3, so arctan(√3) = π/3.",
            "π/3", "π/6", "π/4", "π/2", -0.5));

        questions.Add(Q(T("5.1.1"), "[EXP2] sin(arcsin(0.5)) =",
            D.Easy, "sin and arcsin are inverse functions: sin(arcsin(x)) = x for x ∈ [-1,1]. Answer: 0.5.",
            "0.5", "arcsin(0.5)", "π/6", "1", -1.2));

        questions.Add(Q(T("5.2.2"), "[EXP2] The domain of arccos(x) is:",
            D.Easy, "arccos is defined for x ∈ [-1, 1].",
            "[-1, 1]", "(-∞, +∞)", "[0, 1]", "[-π/2, π/2]", -0.8));

        questions.Add(Q(T("5.1.2"), "[EXP2] arccos(√2/2) =",
            D.Easy, "cos(π/4) = √2/2, so arccos(√2/2) = π/4.",
            "π/4", "π/3", "π/6", "π/2", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 6: TRIG SUM/DIFFERENCE FORMULAS (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("6.1.1"), "[EXP2] sin(A + B) =",
            D.Easy, "sin A cos B + cos A sin B.",
            "sin A cos B + cos A sin B", "sin A sin B + cos A cos B",
            "cos A cos B - sin A sin B", "sin A cos B - cos A sin B", -1.0));

        questions.Add(Q(T("6.1.2"), "[EXP2] cos 2α in terms of cos α only:",
            D.Medium, "cos 2α = 2cos²α - 1.",
            "2cos²α - 1", "1 - 2cos²α", "cos²α - sin²α is also correct but the single-function form is 2cos²α - 1",
            "2sinα cosα", 0.2));

        questions.Add(Q(T("6.1.3"), "[EXP2] sin 2θ =",
            D.Easy, "Double angle: sin 2θ = 2 sin θ cos θ.",
            "2 sin θ cos θ", "sin²θ + cos²θ", "cos²θ - sin²θ", "2cos²θ - 1", -0.8));

        questions.Add(Q(T("6.2.1"), "[EXP2] cos(π/4 - π/6) = cos(π/12). Using the formula:",
            D.Hard, "cos(A-B) = cos A cos B + sin A sin B = (√2/2)(√3/2) + (√2/2)(1/2) = (√6+√2)/4.",
            "(√6 + √2)/4", "(√6 - √2)/4", "√3/2", "(√2 + 1)/4", 0.8));

        questions.Add(Q(T("6.2.2"), "[EXP2] tan(A + B) =",
            D.Medium, "(tan A + tan B)/(1 - tan A · tan B).",
            "(tan A + tan B)/(1 - tan A tan B)", "(tan A - tan B)/(1 + tan A tan B)",
            "tan A + tan B", "tan A · tan B", 0.3));

        questions.Add(Q(T("6.3.1"), "[EXP2] The general solution of sin x = 0 is:",
            D.Easy, "x = nπ, n ∈ Z.",
            "x = nπ, n ∈ Z", "x = 2nπ, n ∈ Z",
            "x = (2n+1)π/2, n ∈ Z", "x = nπ/2, n ∈ Z", -0.5));

        questions.Add(Q(T("6.1.1"), "[EXP2] cos(A - B) =",
            D.Easy, "cos A cos B + sin A sin B.",
            "cos A cos B + sin A sin B", "cos A cos B - sin A sin B",
            "sin A cos B + cos A sin B", "sin A sin B - cos A cos B", -0.8));

        questions.Add(Q(T("6.1.2"), "[EXP2] Given sin α = 4/5, α ∈ (0, π/2), find cos 2α:",
            D.Hard, "cos α = 3/5. cos 2α = 1 - 2sin²α = 1 - 2(16/25) = 1 - 32/25 = -7/25.",
            "-7/25", "7/25", "24/25", "-24/25", 0.8));

        questions.Add(Q(T("6.2.1"), "[EXP2] sin 75° = sin(45° + 30°) =",
            D.Medium, "(√2/2)(√3/2) + (√2/2)(1/2) = (√6+√2)/4.",
            "(√6 + √2)/4", "(√6 - √2)/4", "(√3 + 1)/4", "√3/2", 0.3));

        questions.Add(Q(T("6.3.1"), "[EXP2] How many solutions does sin x = 1/2 have in [0, 2π]?",
            D.Easy, "sin x = 1/2 at x = π/6 and x = 5π/6. Two solutions.",
            "2", "1", "3", "4", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 7: SEQUENCES (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("7.1.1"), "[EXP2] In arithmetic sequence: a₁ = 3, d = 4. Find a₂₀:",
            D.Easy, "a₂₀ = a₁ + 19d = 3 + 76 = 79.",
            "79", "83", "75", "80", -0.5));

        questions.Add(Q(T("7.1.2"), "[EXP2] Sum of first 100 positive integers:",
            D.Easy, "S = 100 × 101/2 = 5050.",
            "5050", "5000", "5100", "10100", -0.8));

        questions.Add(Q(T("7.2.1"), "[EXP2] Geometric sequence: a₁ = 2, q = 3. Find a₅:",
            D.Easy, "a₅ = a₁ · q⁴ = 2 · 81 = 162.",
            "162", "486", "54", "243", -0.5));

        questions.Add(Q(T("7.2.2"), "[EXP2] Sum of geometric series: a₁ = 1, q = 2, n = 8:",
            D.Medium, "S₈ = a₁(q⁸ - 1)/(q - 1) = (256 - 1)/1 = 255.",
            "255", "256", "128", "511", 0.2));

        questions.Add(Q(T("7.3.1"), "[EXP2] aₙ = 2n - 1 defines what type of sequence?",
            D.Easy, "aₙ = 2n - 1: a₁=1, a₂=3, a₃=5. Common difference d=2. Arithmetic.",
            "Arithmetic", "Geometric", "Harmonic", "Fibonacci", -0.8));

        questions.Add(Q(T("7.3.2"), "[EXP2] Σₖ₌₁ⁿ k² =",
            D.Medium, "Sum of squares: n(n+1)(2n+1)/6.",
            "n(n+1)(2n+1)/6", "n²(n+1)²/4", "n(n+1)/2", "(n+1)²/6", 0.4));

        questions.Add(Q(T("7.1.1"), "[EXP2] Arithmetic mean of 3 and 11 is:",
            D.Easy, "(3 + 11)/2 = 7.",
            "7", "14", "8", "33", -1.2));

        questions.Add(Q(T("7.2.1"), "[EXP2] Geometric mean of 4 and 16 is:",
            D.Easy, "√(4 × 16) = √64 = 8.",
            "8", "10", "12", "20", -0.8));

        questions.Add(Q(T("7.1.2"), "[EXP2] In AP: a₃ = 7, a₇ = 19. Find d:",
            D.Medium, "a₇ - a₃ = 4d = 19 - 7 = 12 → d = 3.",
            "3", "4", "6", "12", 0.0));

        questions.Add(Q(T("7.2.2"), "[EXP2] For |q| < 1, the sum of infinite GP is:",
            D.Medium, "S∞ = a₁/(1 - q).",
            "a₁/(1 - q)", "a₁/(q - 1)", "a₁ · q/(1 - q)", "a₁/(1 + q)", 0.2));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 8: COMPLEX NUMBERS (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("8.1.1"), "[EXP2] The imaginary unit i satisfies:",
            D.Easy, "i² = -1 by definition.",
            "i² = -1", "i² = 1", "i = -1", "i = √1", -1.2));

        questions.Add(Q(T("8.1.2"), "[EXP2] (2 + i)(2 - i) =",
            D.Easy, "Difference of squares: 4 - i² = 4 - (-1) = 5.",
            "5", "4", "3", "4 + i", -0.8));

        questions.Add(Q(T("8.2.1"), "[EXP2] The conjugate of z = 3 - 4i is:",
            D.Easy, "Replace i with -i: z̄ = 3 + 4i.",
            "3 + 4i", "3 - 4i", "-3 + 4i", "-3 - 4i", -1.0));

        questions.Add(Q(T("8.2.2"), "[EXP2] |z| where z = 3 + 4i:",
            D.Easy, "|z| = √(9 + 16) = √25 = 5.",
            "5", "7", "25", "√7", -0.8));

        questions.Add(Q(T("8.3.1"), "[EXP2] Express z = 2(cos π/3 + i sin π/3) in algebraic form:",
            D.Medium, "2(1/2 + i√3/2) = 1 + i√3.",
            "1 + i√3", "2 + i√3", "√3 + i", "1 + 2i", 0.2));

        questions.Add(Q(T("8.1.1"), "[EXP2] i⁶ =",
            D.Easy, "i⁶ = i⁴ · i² = 1 · (-1) = -1.",
            "-1", "1", "i", "-i", -0.5));

        questions.Add(Q(T("8.1.2"), "[EXP2] (1 + i)⁴ =",
            D.Hard, "(1+i)² = 2i. (2i)² = 4i² = -4.",
            "-4", "4", "4i", "-4i", 0.6));

        questions.Add(Q(T("8.2.1"), "[EXP2] If z · z̄ = |z|², and z = a + bi, then z · z̄ =",
            D.Easy, "z · z̄ = (a+bi)(a-bi) = a² + b².",
            "a² + b²", "a² - b²", "2ab", "(a + b)²", -0.5));

        questions.Add(Q(T("8.2.2"), "[EXP2] The real part of z = (2+3i)/(1+i) is:",
            D.Hard, "Multiply by conjugate: (2+3i)(1-i)/((1)²+(1)²) = (2-2i+3i-3i²)/2 = (5+i)/2. Real part = 5/2.",
            "5/2", "2", "3/2", "1/2", 0.8));

        questions.Add(Q(T("8.3.1"), "[EXP2] In polar form, multiplication of complex numbers multiplies their moduli and:",
            D.Medium, "We multiply moduli and add arguments.",
            "Adds their arguments", "Multiplies their arguments",
            "Subtracts their arguments", "Divides their arguments", 0.0));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 9: LINES IN PLANE (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("9.1.1"), "[EXP2] The equation of a line with slope 2 passing through (1, 3) is:",
            D.Easy, "y - 3 = 2(x - 1) → y = 2x + 1.",
            "y = 2x + 1", "y = 2x + 3", "y = 2x - 1", "y = x + 2", -0.5));

        questions.Add(Q(T("9.1.2"), "[EXP2] The general form of y = -x + 4 is:",
            D.Easy, "x + y - 4 = 0.",
            "x + y - 4 = 0", "x - y + 4 = 0", "-x + y - 4 = 0", "x + y + 4 = 0", -0.8));

        questions.Add(Q(T("9.2.1"), "[EXP2] Lines y = 3x + 1 and y = -1/3 x + 2 are:",
            D.Easy, "k₁ · k₂ = 3 · (-1/3) = -1 → perpendicular.",
            "Perpendicular", "Parallel", "Coincident", "Intersecting at 60°", -0.5));

        questions.Add(Q(T("9.2.2"), "[EXP2] The distance between parallel lines 2x + y - 3 = 0 and 2x + y + 7 = 0 is:",
            D.Medium, "d = |c₁ - c₂|/√(a²+b²) = |-3-7|/√(4+1) = 10/√5 = 2√5.",
            "2√5", "10", "√5", "4√5", 0.3));

        questions.Add(Q(T("9.1.1"), "[EXP2] A line through (0,0) with slope -1 has equation:",
            D.Easy, "y = -x or equivalently x + y = 0.",
            "y = -x", "y = x", "y = -x + 1", "x = -y + 1", -1.0));

        questions.Add(Q(T("9.2.1"), "[EXP2] Two lines are parallel if and only if they have:",
            D.Easy, "Same slope but different y-intercepts.",
            "Same slope, different intercepts", "Same intercept, different slopes",
            "Slopes that multiply to -1", "Both pass through origin", -0.5));

        questions.Add(Q(T("9.2.2"), "[EXP2] The distance from point (3, 4) to line x = 0 is:",
            D.Easy, "Line x = 0 is the y-axis. Distance = |3| = 3.",
            "3", "4", "5", "7", -1.0));

        questions.Add(Q(T("9.1.2"), "[EXP2] The intercept form of a line passing through (3, 0) and (0, 4) is:",
            D.Easy, "x/a + y/b = 1 → x/3 + y/4 = 1.",
            "x/3 + y/4 = 1", "3x + 4y = 1", "x/4 + y/3 = 1", "4x + 3y = 12", -0.5));

        questions.Add(Q(T("9.1.1"), "[EXP2] The angle of inclination of y = √3 x is:",
            D.Easy, "Slope k = √3 = tan α → α = 60°.",
            "60°", "30°", "45°", "90°", -0.5));

        questions.Add(Q(T("9.2.1"), "[EXP2] The line y = kx + b is perpendicular to y = 2x - 1 when k =",
            D.Easy, "k · 2 = -1 → k = -1/2.",
            "-1/2", "1/2", "-2", "2", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 10: CONIC SECTIONS (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("10.1.1"), "[EXP2] The center and radius of (x-1)² + (y+2)² = 9 are:",
            D.Easy, "Center (1, -2), radius √9 = 3.",
            "Center (1,-2), radius 3", "Center (-1,2), radius 3",
            "Center (1,-2), radius 9", "Center (1,2), radius 3", -0.8));

        questions.Add(Q(T("10.2.1"), "[EXP2] The length of the major axis of x²/16 + y²/9 = 1 is:",
            D.Easy, "a² = 16, a = 4. Major axis = 2a = 8.",
            "8", "4", "16", "10", -0.5));

        questions.Add(Q(T("10.2.2"), "[EXP2] The eccentricity of a circle is:",
            D.Easy, "A circle has e = 0 (both foci at center).",
            "0", "1", "1/2", "Undefined", -1.0));

        questions.Add(Q(T("10.3.1"), "[EXP2] The asymptotes of x²/4 - y²/9 = 1 are:",
            D.Medium, "y = ±(b/a)x = ±(3/2)x.",
            "y = ±3x/2", "y = ±2x/3", "y = ±3x", "y = ±x", 0.3));

        questions.Add(Q(T("10.3.2"), "[EXP2] The difference of distances from any point on a hyperbola to its foci is:",
            D.Easy, "By definition: |PF₁ - PF₂| = 2a.",
            "2a", "2b", "2c", "a + b", -0.5));

        questions.Add(Q(T("10.4.1"), "[EXP2] The vertex of parabola y² = 12x is at:",
            D.Easy, "Standard form y² = 4px. Vertex at origin (0,0).",
            "(0, 0)", "(3, 0)", "(0, 3)", "(12, 0)", -1.0));

        questions.Add(Q(T("10.5.1"), "[EXP2] The length of the latus rectum of y² = 8x is:",
            D.Medium, "y² = 4px → p = 2. Latus rectum = 4p = 8.",
            "8", "4", "2", "16", 0.2));

        questions.Add(Q(T("10.2.1"), "[EXP2] For x²/25 + y²/16 = 1, the semi-minor axis b =",
            D.Easy, "b² = 16, b = 4.",
            "4", "5", "16", "25", -0.8));

        questions.Add(Q(T("10.1.1"), "[EXP2] A point (3, 4) is outside the circle x² + y² = 16 because:",
            D.Easy, "3² + 4² = 25 > 16. Point is outside.",
            "9 + 16 = 25 > 16", "3 + 4 > 4", "3² > 16", "4² > 16", -0.5));

        questions.Add(Q(T("10.4.1"), "[EXP2] Which parabola opens downward?",
            D.Easy, "x² = -4y opens downward (negative coefficient of y).",
            "x² = -4y", "y² = 4x", "x² = 4y", "y² = -4x", -0.8));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 11: PLANE VECTORS (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("11.1.1"), "[EXP2] If a⃗ = (3, 4), then the unit vector in the direction of a⃗ is:",
            D.Medium, "|a⃗| = 5. Unit vector = (3/5, 4/5).",
            "(3/5, 4/5)", "(3, 4)", "(4/5, 3/5)", "(3/4, 1)", 0.0));

        questions.Add(Q(T("11.1.2"), "[EXP2] a⃗ = (1, 2), b⃗ = (3, -1). Then a⃗ + b⃗ =",
            D.Easy, "(1+3, 2+(-1)) = (4, 1).",
            "(4, 1)", "(4, 3)", "(2, 1)", "(3, 1)", -0.8));

        questions.Add(Q(T("11.2.1"), "[EXP2] a⃗ = (2, 1), b⃗ = (1, -2). Then a⃗ · b⃗ =",
            D.Easy, "2·1 + 1·(-2) = 2 - 2 = 0.",
            "0", "4", "-4", "2", -0.5));;

        questions.Add(Q(T("11.2.2"), "[EXP2] The projection of a⃗ = (3, 4) onto b⃗ = (1, 0) is:",
            D.Medium, "proj = (a⃗ · b⃗)/|b⃗| = 3/1 = 3.",
            "3", "4", "5", "7", 0.2));

        questions.Add(Q(T("11.3.1"), "[EXP2] If a⃗ = (2, 3) and k = -2, then ka⃗ =",
            D.Easy, "(-4, -6).",
            "(-4, -6)", "(4, 6)", "(-2, -3)", "(0, 0)", -1.0));

        questions.Add(Q(T("11.1.1"), "[EXP2] Two vectors a⃗ = (1, 2) and b⃗ = (2, 4) are:",
            D.Easy, "b⃗ = 2a⃗, so they are parallel (collinear).",
            "Parallel", "Perpendicular", "Neither", "Equal", -0.5));

        questions.Add(Q(T("11.1.2"), "[EXP2] |a⃗ - b⃗| where a⃗ = (4, 0), b⃗ = (1, 0) is:",
            D.Easy, "a⃗ - b⃗ = (3, 0). |a⃗ - b⃗| = 3.",
            "3", "5", "4", "1", -0.8));

        questions.Add(Q(T("11.2.1"), "[EXP2] If a⃗ · b⃗ = |a⃗||b⃗|, the angle between them is:",
            D.Easy, "cos θ = 1 → θ = 0. Vectors are in the same direction.",
            "0", "π/2", "π", "π/4", -0.5));

        questions.Add(Q(T("11.2.2"), "[EXP2] The area of triangle with sides a⃗ = (1, 0) and b⃗ = (0, 1) is:",
            D.Easy, "Area = (1/2)|x₁y₂ - x₂y₁| = (1/2)|1·1 - 0·0| = 1/2.",
            "1/2", "1", "√2/2", "2", -0.5));

        questions.Add(Q(T("11.3.1"), "[EXP2] The midpoint vector of A(x₁,y₁) and B(x₂,y₂) is:",
            D.Easy, "Midpoint M = ((x₁+x₂)/2, (y₁+y₂)/2).",
            "((x₁+x₂)/2, (y₁+y₂)/2)", "(x₁+x₂, y₁+y₂)",
            "((x₁-x₂)/2, (y₁-y₂)/2)", "(x₁x₂, y₁y₂)", -0.8));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 12: SPACE VECTORS (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("12.1.1"), "[EXP2] The magnitude of a⃗ = (1, 2, 2) is:",
            D.Easy, "|a⃗| = √(1 + 4 + 4) = 3.",
            "3", "√5", "5", "√8", -0.8));

        questions.Add(Q(T("12.1.2"), "[EXP2] a⃗ = (1, -1, 0), b⃗ = (0, 1, -1). Then a⃗ · b⃗ =",
            D.Easy, "0 + (-1) + 0 = -1.",
            "-1", "0", "1", "2", -0.5));

        questions.Add(Q(T("12.2.1"), "[EXP2] a⃗ × b⃗ where a⃗ = (1,0,0), b⃗ = (0,1,0) is:",
            D.Medium, "i×j = k → (0, 0, 1).",
            "(0, 0, 1)", "(0, 0, -1)", "(1, 1, 0)", "(0, 1, 0)", 0.2));

        questions.Add(Q(T("12.2.2"), "[EXP2] The cross product a⃗ × a⃗ =",
            D.Easy, "Any vector crossed with itself gives the zero vector.",
            "0⃗", "a⃗", "|a⃗|²", "2a⃗", -1.0));

        questions.Add(Q(T("12.1.1"), "[EXP2] The distance between points A(1,0,0) and B(0,1,0) is:",
            D.Easy, "d = √((1-0)² + (0-1)² + (0-0)²) = √2.",
            "√2", "1", "2", "√3", -0.5));

        questions.Add(Q(T("12.1.2"), "[EXP2] Two space vectors are perpendicular if their dot product is:",
            D.Easy, "Zero dot product means perpendicular.",
            "0", "1", "-1", "Undefined", -1.0));

        questions.Add(Q(T("12.2.1"), "[EXP2] |a⃗ × b⃗| gives the area of:",
            D.Medium, "The cross product magnitude equals the area of the parallelogram spanned by a⃗ and b⃗.",
            "The parallelogram formed by a⃗ and b⃗", "The triangle formed by a⃗ and b⃗",
            "The rectangle formed by a⃗ and b⃗", "The circle with diameter |a⃗|+|b⃗|", 0.0));

        questions.Add(Q(T("12.1.1"), "[EXP2] The unit vector in the z-direction is:",
            D.Easy, "k̂ = (0, 0, 1).",
            "(0, 0, 1)", "(1, 0, 0)", "(0, 1, 0)", "(1, 1, 1)", -1.2));

        questions.Add(Q(T("12.2.2"), "[EXP2] The scalar triple product a⃗ · (b⃗ × c⃗) gives:",
            D.Hard, "It gives the signed volume of the parallelepiped formed by the three vectors.",
            "Volume of parallelepiped", "Area of parallelogram",
            "Length of projection", "Angle between vectors", 0.8));

        questions.Add(Q(T("12.1.2"), "[EXP2] Direction cosines of a⃗ = (1, 1, 1) are:",
            D.Medium, "|a⃗| = √3. Direction cosines: (1/√3, 1/√3, 1/√3).",
            "(1/√3, 1/√3, 1/√3)", "(1, 1, 1)", "(1/3, 1/3, 1/3)", "(√3, √3, √3)", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 13: SPACE PLANES & LINES (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("13.1.1"), "[EXP2] The equation of a plane with normal vector (1, 0, 0) through origin is:",
            D.Easy, "1·x + 0·y + 0·z = 0 → x = 0 (the yz-plane).",
            "x = 0", "y = 0", "z = 0", "x + y + z = 0", -0.8));

        questions.Add(Q(T("13.1.2"), "[EXP2] Two planes are parallel if their normal vectors are:",
            D.Easy, "Parallel normals → parallel planes.",
            "Parallel", "Perpendicular", "Equal in magnitude", "Opposite", -0.5));

        questions.Add(Q(T("13.2.1"), "[EXP2] The angle between planes x + y + z = 1 and x - y = 0 is found by:",
            D.Medium, "cos θ = |n₁ · n₂|/(|n₁||n₂|). n₁ = (1,1,1), n₂ = (1,-1,0). Dot = 0 → θ = π/2.",
            "π/2", "0", "π/4", "π/3", 0.2));

        questions.Add(Q(T("13.2.2"), "[EXP2] The distance from point (1, 2, 3) to plane x + y + z = 0 is:",
            D.Medium, "d = |1+2+3|/√3 = 6/√3 = 2√3.",
            "2√3", "6", "√3", "3√3", 0.3));

        questions.Add(Q(T("13.1.1"), "[EXP2] Three non-collinear points determine:",
            D.Easy, "Three non-collinear points determine exactly one plane.",
            "Exactly one plane", "Two planes", "A line", "No plane", -1.0));

        questions.Add(Q(T("13.1.2"), "[EXP2] A line perpendicular to a plane is perpendicular to:",
            D.Easy, "It is perpendicular to every line in the plane that passes through the foot.",
            "Every line in the plane through the foot", "Only one line in the plane",
            "Only the normal", "No lines in the plane", -0.5));

        questions.Add(Q(T("13.2.1"), "[EXP2] The parametric equation of a line through (1,2,3) with direction (1,0,-1) is:",
            D.Medium, "x = 1+t, y = 2, z = 3-t.",
            "x=1+t, y=2, z=3-t", "x=t, y=2t, z=3t",
            "x=1, y=2+t, z=3+t", "x=1-t, y=2+t, z=3", 0.2));

        questions.Add(Q(T("13.2.2"), "[EXP2] Two lines in space that do not intersect and are not parallel are called:",
            D.Easy, "Skew lines.",
            "Skew lines", "Parallel lines", "Concurrent lines", "Coplanar lines", -0.5));

        questions.Add(Q(T("13.1.1"), "[EXP2] The normal vector to plane 2x - 3y + z = 5 is:",
            D.Easy, "Read coefficients of x, y, z: (2, -3, 1).",
            "(2, -3, 1)", "(2, 3, 1)", "(-2, 3, -1)", "(5, 0, 0)", -0.8));

        questions.Add(Q(T("13.1.2"), "[EXP2] The intersection of two non-parallel planes is:",
            D.Easy, "Two non-parallel planes intersect in a line.",
            "A line", "A point", "A plane", "Empty set", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 14: LIMITS (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("14.1.1"), "[EXP2] lim(x→0) sin x / x =",
            D.Easy, "Famous limit: equals 1.",
            "1", "0", "∞", "sin 1", -1.0));

        questions.Add(Q(T("14.1.2"), "[EXP2] lim(x→∞) (1 + 1/x)^x =",
            D.Medium, "The definition of e ≈ 2.718.",
            "e", "1", "∞", "0", 0.0));

        questions.Add(Q(T("14.2.1"), "[EXP2] lim(x→2) (x² - 4)/(x - 2) =",
            D.Easy, "(x-2)(x+2)/(x-2) = x+2. At x = 2: limit = 4.",
            "4", "0", "2", "∞", -0.5));

        questions.Add(Q(T("14.2.2"), "[EXP2] lim(x→0) (1 - cos x)/x² =",
            D.Hard, "Using L'Hôpital or Taylor: (1 - cos x)/x² → 1/2.",
            "1/2", "0", "1", "2", 0.8));

        questions.Add(Q(T("14.3.1"), "[EXP2] A function f is continuous at x = a if:",
            D.Medium, "f(a) exists, lim(x→a) f(x) exists, and lim(x→a) f(x) = f(a).",
            "All three conditions must hold", "Only f(a) must exist",
            "Only the limit must exist", "f must be differentiable at a", 0.2));

        questions.Add(Q(T("14.1.1"), "[EXP2] lim(x→3) 5 =",
            D.Easy, "The limit of a constant is the constant itself: 5.",
            "5", "3", "15", "0", -1.2));

        questions.Add(Q(T("14.1.2"), "[EXP2] lim(x→∞) 1/x =",
            D.Easy, "As x → ∞, 1/x → 0.",
            "0", "1", "∞", "-∞", -1.0));

        questions.Add(Q(T("14.2.1"), "[EXP2] lim(x→0) (e^x - 1)/x =",
            D.Medium, "This is a standard limit equal to 1.",
            "1", "e", "0", "∞", 0.2));

        questions.Add(Q(T("14.2.2"), "[EXP2] lim(x→∞) (3x² + x)/(x² + 1) =",
            D.Medium, "Divide by x²: (3 + 1/x)/(1 + 1/x²) → 3.",
            "3", "1", "∞", "0", 0.0));

        questions.Add(Q(T("14.3.1"), "[EXP2] If lim(x→a⁻) f(x) ≠ lim(x→a⁺) f(x), then:",
            D.Easy, "The two-sided limit does not exist; f has a jump discontinuity.",
            "The limit does not exist", "The limit equals f(a)",
            "f is continuous at a", "The limit is ∞", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 15: DERIVATIVES (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("15.1.1"), "[EXP2] d/dx (x⁵) =",
            D.Easy, "Power rule: 5x⁴.",
            "5x⁴", "x⁴", "4x⁵", "5x⁵", -1.0));

        questions.Add(Q(T("15.1.2"), "[EXP2] d/dx (e^x) =",
            D.Easy, "The derivative of e^x is e^x.",
            "eˣ", "xeˣ⁻¹", "1", "eˣ⁺¹", -1.2));

        questions.Add(Q(T("15.2.1"), "[EXP2] d/dx (sin x) =",
            D.Easy, "cos x.",
            "cos x", "-sin x", "sin x", "-cos x", -1.0));

        questions.Add(Q(T("15.2.2"), "[EXP2] d/dx (ln x) =",
            D.Easy, "1/x.",
            "1/x", "ln x", "x", "1/(x ln x)", -0.8));

        questions.Add(Q(T("15.2.3"), "[EXP2] Using product rule: d/dx (x · sin x) =",
            D.Medium, "sin x + x cos x.",
            "sin x + x cos x", "x cos x", "cos x", "x sin x + cos x", 0.0));

        questions.Add(Q(T("15.1.1"), "[EXP2] d/dx (3x² - 2x + 7) =",
            D.Easy, "6x - 2.",
            "6x - 2", "6x + 2", "3x² - 2", "6x - 2x", -0.8));

        questions.Add(Q(T("15.1.2"), "[EXP2] d/dx (1/x) =",
            D.Easy, "d/dx (x⁻¹) = -x⁻² = -1/x².",
            "-1/x²", "1/x²", "-1/x", "ln x", -0.5));

        questions.Add(Q(T("15.2.1"), "[EXP2] d/dx (cos x) =",
            D.Easy, "-sin x.",
            "-sin x", "sin x", "cos x", "-cos x", -1.0));

        questions.Add(Q(T("15.2.2"), "[EXP2] Chain rule: d/dx (sin(3x)) =",
            D.Medium, "3cos(3x).",
            "3cos(3x)", "cos(3x)", "3sin(3x)", "-3cos(3x)", 0.0));

        questions.Add(Q(T("15.2.3"), "[EXP2] d/dx (x³eˣ) =",
            D.Hard, "Product rule: 3x²eˣ + x³eˣ = eˣ(3x² + x³) = x²eˣ(3 + x).",
            "x²eˣ(3 + x)", "3x²eˣ", "x³eˣ", "eˣ(x³ + 3)", 0.6));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 16: APPLICATIONS OF DERIVATIVES (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("16.1.1"), "[EXP2] If f'(x) > 0 on (a, b), then f is:",
            D.Easy, "Increasing on (a, b).",
            "Increasing", "Decreasing", "Constant", "Concave up", -0.8));

        questions.Add(Q(T("16.1.2"), "[EXP2] At a local maximum, f'(x) =",
            D.Easy, "At a local extremum (if f is differentiable), f'(x) = 0.",
            "0", "1", "Undefined", "Positive", -0.5));

        questions.Add(Q(T("16.2.1"), "[EXP2] f(x) = x³ - 3x. Find the critical points:",
            D.Medium, "f'(x) = 3x² - 3 = 0 → x² = 1 → x = ±1.",
            "x = 1 and x = -1", "x = 0 and x = 3", "x = 0 only", "x = √3 and x = -√3", 0.0));

        questions.Add(Q(T("16.2.2"), "[EXP2] Second derivative test: if f'(c) = 0 and f''(c) > 0, then c is:",
            D.Easy, "A local minimum.",
            "A local minimum", "A local maximum", "An inflection point", "Neither", -0.5));

        questions.Add(Q(T("16.3.1"), "[EXP2] The tangent line to y = x² at (1, 1) has equation:",
            D.Medium, "y' = 2x. At x = 1: y' = 2. y - 1 = 2(x - 1) → y = 2x - 1.",
            "y = 2x - 1", "y = x + 1", "y = 2x", "y = x²", 0.0));

        questions.Add(Q(T("16.1.1"), "[EXP2] If f''(x) > 0 on (a, b), then f is:",
            D.Medium, "Concave up (convex) on (a, b).",
            "Concave up", "Concave down", "Increasing", "Decreasing", 0.2));

        questions.Add(Q(T("16.1.2"), "[EXP2] An inflection point of f(x) occurs where:",
            D.Medium, "f'' changes sign (concavity changes).",
            "f'' changes sign", "f' = 0", "f = 0", "f'' = 0 always", 0.3));

        questions.Add(Q(T("16.2.1"), "[EXP2] Find the maximum of f(x) = -x² + 4x - 1 on ℝ:",
            D.Easy, "f'(x) = -2x + 4 = 0 → x = 2. f(2) = -4 + 8 - 1 = 3.",
            "3", "4", "2", "1", -0.5));

        questions.Add(Q(T("16.2.2"), "[EXP2] L'Hôpital's rule applies when the limit has form:",
            D.Medium, "0/0 or ∞/∞ indeterminate forms.",
            "0/0 or ∞/∞", "0 · ∞", "1/0", "Any form", 0.2));

        questions.Add(Q(T("16.3.1"), "[EXP2] The velocity of a particle with position s(t) = t³ - 6t at t = 2 is:",
            D.Medium, "v(t) = s'(t) = 3t² - 6. At t = 2: v = 12 - 6 = 6.",
            "6", "2", "12", "0", 0.0));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 17: PERMUTATIONS & COMBINATIONS (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("17.1.1"), "[EXP2] 5! =",
            D.Easy, "5! = 5 × 4 × 3 × 2 × 1 = 120.",
            "120", "60", "24", "720", -1.0));

        questions.Add(Q(T("17.1.2"), "[EXP2] P(6, 2) =",
            D.Easy, "P(6,2) = 6!/(6-2)! = 6 × 5 = 30.",
            "30", "15", "12", "36", -0.5));

        questions.Add(Q(T("17.1.3"), "[EXP2] How many 3-letter words from {A, B, C, D, E} without repetition?",
            D.Easy, "P(5,3) = 5 × 4 × 3 = 60.",
            "60", "10", "125", "15", -0.5));

        questions.Add(Q(T("17.2.1"), "[EXP2] C(7, 3) =",
            D.Easy, "C(7,3) = 7!/(3!4!) = 35.",
            "35", "21", "42", "210", -0.5));

        questions.Add(Q(T("17.2.2"), "[EXP2] In the expansion of (a + b)⁴, the coefficient of a²b² is:",
            D.Medium, "C(4,2) = 6.",
            "6", "4", "8", "12", 0.0));

        questions.Add(Q(T("17.1.1"), "[EXP2] 0! =",
            D.Easy, "By convention, 0! = 1.",
            "1", "0", "Undefined", "∞", -1.0));

        questions.Add(Q(T("17.1.2"), "[EXP2] P(n, 1) =",
            D.Easy, "P(n,1) = n!/(n-1)! = n.",
            "n", "1", "n!", "n-1", -0.8));

        questions.Add(Q(T("17.2.1"), "[EXP2] C(n, 0) =",
            D.Easy, "C(n,0) = 1 (one way to choose nothing).",
            "1", "0", "n", "n!", -1.0));

        questions.Add(Q(T("17.2.2"), "[EXP2] C(n, n) =",
            D.Easy, "C(n,n) = 1 (one way to choose all).",
            "1", "n", "n!", "0", -1.0));

        questions.Add(Q(T("17.1.3"), "[EXP2] How many ways to arrange 4 people in a line?",
            D.Easy, "4! = 24.",
            "24", "16", "12", "4", -0.8));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 18: RANDOM EVENTS & PROBABILITY (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("18.1.1"), "[EXP2] The probability of an impossible event is:",
            D.Easy, "P(impossible event) = 0.",
            "0", "1", "-1", "0.5", -1.2));

        questions.Add(Q(T("18.1.2"), "[EXP2] The probability of a certain (sure) event is:",
            D.Easy, "P(certain event) = 1.",
            "1", "0", "0.5", "∞", -1.2));

        questions.Add(Q(T("18.2.1"), "[EXP2] A fair die is thrown. P(even number) =",
            D.Easy, "Even: {2, 4, 6}. P = 3/6 = 1/2.",
            "1/2", "1/3", "1/6", "2/3", -0.8));

        questions.Add(Q(T("18.2.2"), "[EXP2] P(A ∪ B) for mutually exclusive events A, B =",
            D.Easy, "P(A) + P(B) (addition rule for mutually exclusive events).",
            "P(A) + P(B)", "P(A) · P(B)", "P(A) + P(B) - P(A ∩ B)", "P(A|B)", -0.5));

        questions.Add(Q(T("18.3.1"), "[EXP2] For independent events A and B, P(A ∩ B) =",
            D.Easy, "P(A) · P(B).",
            "P(A) · P(B)", "P(A) + P(B)", "P(A|B)", "P(A) - P(B)", -0.8));

        questions.Add(Q(T("18.1.1"), "[EXP2] If P(A) = 0.3, then P(Ā) =",
            D.Easy, "P(Ā) = 1 - P(A) = 0.7.",
            "0.7", "0.3", "1.3", "0", -1.0));

        questions.Add(Q(T("18.1.2"), "[EXP2] Probability must satisfy 0 ≤ P(A) ≤ ?",
            D.Easy, "0 ≤ P(A) ≤ 1.",
            "1", "∞", "100", "0.5", -1.2));

        questions.Add(Q(T("18.2.1"), "[EXP2] Drawing a card from a standard deck: P(heart) =",
            D.Easy, "13 hearts out of 52 cards: P = 1/4.",
            "1/4", "1/13", "1/2", "1/52", -0.5));

        questions.Add(Q(T("18.2.2"), "[EXP2] P(A ∪ B) = P(A) + P(B) - P(A ∩ B) is called:",
            D.Easy, "The inclusion-exclusion principle (addition rule).",
            "Inclusion-exclusion principle", "Bayes' theorem",
            "Multiplication rule", "Total probability law", -0.5));

        questions.Add(Q(T("18.3.1"), "[EXP2] Conditional probability P(A|B) =",
            D.Medium, "P(A ∩ B) / P(B), provided P(B) > 0.",
            "P(A ∩ B)/P(B)", "P(A) · P(B)", "P(A ∪ B)/P(B)", "P(B)/P(A)", 0.2));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 19: RANDOM VARIABLES (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("19.1.1"), "[EXP2] A random variable X that takes values 0, 1, 2, ... is called:",
            D.Easy, "Discrete random variable.",
            "Discrete", "Continuous", "Constant", "Deterministic", -0.8));

        questions.Add(Q(T("19.1.2"), "[EXP2] The expected value of a fair die roll is:",
            D.Easy, "E(X) = (1+2+3+4+5+6)/6 = 3.5.",
            "3.5", "3", "4", "21", -0.5));

        questions.Add(Q(T("19.2.1"), "[EXP2] Var(X) = E(X²) - [E(X)]² is the formula for:",
            D.Easy, "Variance.",
            "Variance", "Standard deviation", "Mean", "Median", -0.5));

        questions.Add(Q(T("19.2.2"), "[EXP2] If Var(X) = 9, then the standard deviation σ =",
            D.Easy, "σ = √Var(X) = √9 = 3.",
            "3", "9", "81", "√3", -0.8));

        questions.Add(Q(T("19.3.1"), "[EXP2] In a binomial distribution B(n, p), E(X) =",
            D.Easy, "E(X) = np.",
            "np", "n/p", "p/n", "n(1-p)", -0.5));

        questions.Add(Q(T("19.3.2"), "[EXP2] The variance of B(n, p) is:",
            D.Medium, "Var(X) = np(1-p).",
            "np(1-p)", "np", "n²p", "p(1-p)", 0.0));

        questions.Add(Q(T("19.1.1"), "[EXP2] E(aX + b) =",
            D.Easy, "aE(X) + b (linearity of expectation).",
            "aE(X) + b", "aE(X)", "E(X) + b", "a²E(X) + b", -0.5));

        questions.Add(Q(T("19.1.2"), "[EXP2] A coin is tossed 3 times. X = number of heads. P(X = 2) =",
            D.Medium, "C(3,2)(1/2)²(1/2)¹ = 3/8.",
            "3/8", "1/4", "1/2", "1/8", 0.2));

        questions.Add(Q(T("19.2.1"), "[EXP2] Var(aX + b) =",
            D.Medium, "a²Var(X). The constant b does not affect variance.",
            "a²Var(X)", "aVar(X) + b", "a²Var(X) + b²", "Var(X)", 0.3));

        questions.Add(Q(T("19.2.2"), "[EXP2] The sum of all probabilities in a probability distribution equals:",
            D.Easy, "1.",
            "1", "0", "0.5", "It depends", -1.2));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 20: STATISTICS (+10)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("20.1.1"), "[EXP2] The mean of 2, 4, 6, 8, 10 is:",
            D.Easy, "(2+4+6+8+10)/5 = 30/5 = 6.",
            "6", "5", "8", "10", -1.0));

        questions.Add(Q(T("20.1.2"), "[EXP2] The median of {3, 7, 1, 5, 9} is:",
            D.Easy, "Sorted: 1, 3, 5, 7, 9. Middle value = 5.",
            "5", "3", "7", "25", -0.8));

        questions.Add(Q(T("20.2.1"), "[EXP2] The mode of {2, 3, 3, 4, 5, 3, 6} is:",
            D.Easy, "3 appears most frequently.",
            "3", "2", "4", "5", -1.0));

        questions.Add(Q(T("20.2.2"), "[EXP2] The range of {1, 5, 3, 9, 7} is:",
            D.Easy, "Range = max - min = 9 - 1 = 8.",
            "8", "5", "9", "4", -0.8));;

        questions.Add(Q(T("20.3.1"), "[EXP2] The sample variance formula uses n-1 because:",
            D.Hard, "Using n-1 (Bessel's correction) gives an unbiased estimator of population variance.",
            "Bessel's correction for unbiased estimation", "To make the formula simpler",
            "Because n is too large", "To increase the variance", 0.8));

        questions.Add(Q(T("20.3.2"), "[EXP2] If r is close to +1, the scatter plot shows:",
            D.Easy, "Strong positive linear correlation — points cluster around an upward line.",
            "Strong positive linear trend", "No correlation",
            "Strong negative linear trend", "Perfect circle", -0.5));

        questions.Add(Q(T("20.1.1"), "[EXP2] The weighted mean of 60 (weight 2) and 90 (weight 3) is:",
            D.Easy, "(60×2 + 90×3)/(2+3) = (120+270)/5 = 78.",
            "78", "75", "80", "85", -0.5));

        questions.Add(Q(T("20.1.2"), "[EXP2] In a frequency histogram, the y-axis shows:",
            D.Easy, "Frequency (or frequency density).",
            "Frequency or frequency density", "The data values",
            "Cumulative frequency", "Standard deviation", -0.5));

        questions.Add(Q(T("20.2.1"), "[EXP2] A dataset can have how many modes?",
            D.Easy, "Zero (no repeated values), one, or multiple modes.",
            "Zero, one, or more", "Exactly one", "At most two", "None", -0.5));

        questions.Add(Q(T("20.3.1"), "[EXP2] The standard deviation is measured in:",
            D.Easy, "The same units as the original data (unlike variance which is squared units).",
            "Same units as the data", "Squared units", "No units", "Percentage", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // Save all questions
        // ═══════════════════════════════════════════════════════════════

        await _context.Questions.AddRangeAsync(questions);
        await _context.SaveChangesAsync();
    }

    private static class D
    {
        public const QuestionDifficulty Easy = QuestionDifficulty.Easy;
        public const QuestionDifficulty Medium = QuestionDifficulty.Medium;
        public const QuestionDifficulty Hard = QuestionDifficulty.Hard;
    }

    private static Question Q(
        int topicId, string text, QuestionDifficulty difficulty, string explanation,
        string correct, string wrong1, string wrong2, string wrong3,
        double difficultyParam = 0.0, double discriminationParam = 1.0, double guessParam = 0.25)
    {
        return new Question
        {
            TopicId = topicId,
            Text = text,
            Difficulty = difficulty,
            Explanation = explanation,
            DifficultyParam = difficultyParam,
            DiscriminationParam = discriminationParam,
            GuessParam = guessParam,
            AnswerOptions = new List<AnswerOption>
            {
                new AnswerOption { Text = correct, IsCorrect = true },
                new AnswerOption { Text = wrong1, IsCorrect = false },
                new AnswerOption { Text = wrong2, IsCorrect = false },
                new AnswerOption { Text = wrong3, IsCorrect = false },
            }
        };
    }
}
