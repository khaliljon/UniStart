using Microsoft.EntityFrameworkCore;
using UniStart.Domain.Entities;

namespace UniStart.Infrastructure.Data;

/// <summary>
/// Seeds ~250 multiple-choice questions for CSCA Mathematics (20 chapters).
/// Each question has exactly 4 answer options, difficulty level, explanation, and IRT parameters.
/// </summary>
public class CscaMathQuestionSeeder
{
    private readonly UniStartDbContext _context;

    public CscaMathQuestionSeeder(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Skip if CSCA math questions already seeded (check for a known question)
        var mathSection = await _context.ExamSections
            .FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Mathematics") && !s.Name.Contains("(CN)"));
        if (mathSection == null) return;

        var existingCscaMathQuestions = await _context.Questions
            .AnyAsync(q => q.Topic.SectionId == mathSection.Id);
        if (existingCscaMathQuestions) return;

        // Resolve topics by name prefix
        var topics = await _context.Topics
            .Where(t => t.SectionId == mathSection.Id)
            .ToDictionaryAsync(t => t.Name.Split(' ')[0], t => t.Id);

        // Helper to get topicId by prefix (e.g., "1.1.1")
        int T(string prefix) => topics.TryGetValue(prefix, out var id) ? id : topics.Values.First();

        var questions = new List<Question>();

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 1: SETS  (Topics 1.1.x – 1.4.x)
        // ═══════════════════════════════════════════════════════════════

        // --- 1.1 Elements of Sets ---
        questions.Add(Q(T("1.1.1"), "Let A = {x | -5 < x ≤ 5}. Which statement is correct?",
            D.Easy, "4.3 is between -5 and 5 (exclusive of -5, inclusive of 5), so 4.3 ∈ A. -5 is not included, 5/3 ∈ A, so option C is correct.",
            "4.3 ∈ A", "5/3 ∉ A", "-7/3 ∉ A", "-5 ∈ A", -1.0));

        questions.Add(Q(T("1.1.2"), "Let B = {x ∈ Z | -4 ≤ x < 2}. Which is correct?",
            D.Easy, "B contains integers from -4 to 1. -2.5 is not an integer so -2.5 ∉ B is correct.",
            "-2.5 ∉ B", "-4 ∉ B", "2 ∈ B", "1.5 ∈ B", -1.2));

        questions.Add(Q(T("1.1.3"), "{x ∈ Z⁺ | x ≤ 4} equals:",
            D.Easy, "Z⁺ = {1, 2, 3, ...}. Integers ≤ 4 are {1, 2, 3, 4}.",
            "{1, 2, 3, 4}", "{0, 1, 2, 3, 4}", "{0, 1, 2, 3, 4, 5}", "{1, 2, 3, 4, 5}", -1.5));

        questions.Add(Q(T("1.1.3"), "Which statement is correct?",
            D.Medium, "If a ∈ Z⁺ then a is a positive integer, which is always a rational number. So a ∈ Q is true.",
            "If a ∈ Z⁺, then a ∈ Q", "If a ∈ N, b ∈ N, then a - b ∈ N",
            "If a ≥ 0, then a ∈ N", "If a ∈ Z, then a ∉ Q", 0.3));

        // --- 1.2 Subsets ---
        questions.Add(Q(T("1.2.1"), "M = {2}, N = {0, 2, 4}. Which is correct?",
            D.Easy, "{2} is a subset of {0, 2, 4} because every element of M is in N. M ⊆ N.",
            "M ⊆ N", "M ∈ N", "N ⊆ M", "M = N", -0.8));

        questions.Add(Q(T("1.2.2"), "Which pair of sets M and N are equal?",
            D.Medium, "M = {x ∈ N | -1 < x ≤ 1} = {0, 1}. But N = {1}. Actually ∛(x³) = x for all real x, so M = {x | x ∈ R} and y = x are identical → M = N for option C: {x ∈ N | -1 < x ≤ 1} = {0, 1} ≠ {1}. For D: √4 = 2, so N = {3, 1, 2} = {1, 2, 3} = M ✓.",
            "M = {1, 2, 3}, N = {3, 1, √4}", "M = {π}, N = {3.14}",
            "M = {2, 3}, N = {(2, 3)}", "M = {x ∈ N | -1 < x ≤ 1}, N = {1}", 0.5));

        questions.Add(Q(T("1.2.3"), "How many subsets does the set {a, b, c} have?",
            D.Easy, "A set with n elements has 2ⁿ subsets. Here n = 3, so 2³ = 8 subsets.",
            "8", "6", "7", "9", -1.0));

        // --- 1.3 Operations on Sets ---
        questions.Add(Q(T("1.2.1"), "U = {0,1,2,3,4,5,6,7,8}, M = {1,2,4,5}, N = {0,3,5,7}. Then ∁_U(M ∪ N) = ?",
            D.Medium, "M ∪ N = {0,1,2,3,4,5,7}. ∁_U(M ∪ N) = {6, 8}.",
            "{6, 8}", "{5, 7}", "{4, 6, 7}", "{1, 3, 5, 6, 8}", 0.2));

        questions.Add(Q(T("1.2.2"), "U = {1,2,3,4,5,6,7}, A = {2,3,4,5}, B = {2,3,6,7}. Then B ∩ (∁_U A) = ?",
            D.Medium, "∁_U A = {1, 6, 7}. B ∩ {1, 6, 7} = {6, 7}.",
            "{6, 7}", "{1, 6}", "{1, 7}", "{1, 6, 7}", 0.3));

        // --- 1.4 Set Properties ---
        questions.Add(Q(T("1.4.3"), "If A ∪ B = A, which must be true?",
            D.Medium, "A ∪ B = A means every element of B is already in A, i.e., B ⊆ A.",
            "B ⊆ A", "A ⊆ B", "A = ∅", "B = ∅", 0.4));

        questions.Add(Q(T("1.4.4"), "A = {x | x² - 4 = 0}, B = {-2, 2, 3}. Then A ∩ B = ?",
            D.Easy, "A = {-2, 2}. A ∩ B = {-2, 2}.",
            "{-2, 2}", "{2, 3}", "{-2, 2, 3}", "{2}", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 2: INEQUALITIES  (Topics 2.1.x – 2.3.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("2.1.1"), "Given a < b < 0, c > 0, which is correct?",
            D.Easy, "a < b, dividing by c (positive): a/c < b/c.",
            "a/c < b/c", "a - c < b - c", "ac > bc", "c/a < c/b", -0.5));

        questions.Add(Q(T("2.1.2"), "The solution set of x² - 5x - 14 < 0 is:",
            D.Medium, "x² - 5x - 14 = (x - 7)(x + 2) = 0, roots: x = -2, x = 7. Since a > 0, for < 0: -2 < x < 7.",
            "(-2, 7)", "(-∞, -2) ∪ (7, +∞)", "(-7, 2)", "(-∞, -7) ∪ (2, +∞)", 0.2));

        questions.Add(Q(T("2.1.2"), "The solution set of -2x² + 7x - 3 ≤ 0 is:",
            D.Medium, "Multiply by -1: 2x² - 7x + 3 ≥ 0. Roots: x = 1/2, x = 3. For ≥ 0 with a > 0: x ≤ 1/2 or x ≥ 3.",
            "(-∞, 1/2] ∪ [3, +∞)", "[1/2, 3]", "[-3, 1/2]", "(-∞, -3] ∪ [-1/2, +∞)", 0.5));

        questions.Add(Q(T("2.1.2"), "The solution set of x² - 12x + 36 ≥ 0 is:",
            D.Medium, "x² - 12x + 36 = (x - 6)² ≥ 0 is true for all real x. Solution: R.",
            "R", "{x | x = 6}", "{x | x ≠ 6}", "∅", 0.3));

        questions.Add(Q(T("2.2.1"), "If -4 < x < 7, then the range of 1 - 2x is:",
            D.Easy, "-4 < x < 7 → -14 < 2x < 8 → -8 < -2x < 14 → -7 < 1 - 2x < 15. But boundary: x = -4 → 1-2(-4) = 9; x = 7 → 1-14 = -13. So -13 < 1 - 2x < 9.",
            "-13 < 1 - 2x < 9", "-7 < 1 - 2x < 15", "-8 < 1 - 2x < 14", "-13 ≤ 1 - 2x ≤ 9", -0.3));

        questions.Add(Q(T("2.3.1"), "The solution set of (x + 2)(x - 3) > 0 is:",
            D.Easy, "Roots: x = -2, x = 3. For product > 0: x < -2 or x > 3.",
            "(-∞, -2) ∪ (3, +∞)", "(-2, 3)", "(-3, 2)", "(-∞, -3) ∪ (2, +∞)", -0.7));

        questions.Add(Q(T("2.3.1"), "The solution set of (x - 2)(x - 7) ≤ 0 is:",
            D.Easy, "Roots: x = 2, x = 7. For product ≤ 0: 2 ≤ x ≤ 7.",
            "[2, 7]", "(-∞, 2] ∪ [7, +∞)", "(2, 7)", "[-7, -2]", -0.5));

        questions.Add(Q(T("2.3.2"), "The solution set of (2x - 5)/(3x - 7) > 0 is:",
            D.Medium, "Critical points: x = 5/2, x = 7/3. Same sign when x < 7/3 or x > 5/2.",
            "(-∞, 7/3) ∪ (5/2, +∞)", "(7/3, 5/2)", "(-∞, 5/2) ∪ (7/3, +∞)", "(5/2, 7/3)", 0.6));

        questions.Add(Q(T("2.1.3"), "Given a > b > 0, which is correct?",
            D.Medium, "If a > b > 0, then √a > √b (property 8 of inequalities).",
            "√a > √b", "1/a > 1/b", "a² < b²", "-a > -b", 0.2));

        questions.Add(Q(T("2.1.1"), "If a > b and c > d, then which must be true?",
            D.Easy, "Property 5: a > b and c > d implies a + c > b + d.",
            "a + c > b + d", "ac > bd", "a - c > b - d", "a/c > b/d", -0.3));

        questions.Add(Q(T("2.2.2"), "The solution set of x² + 2x + 4 > 0 is:",
            D.Medium, "Δ = 4 - 16 = -12 < 0, a = 1 > 0. The parabola is always above the x-axis. Solution: R.",
            "R", "∅", "{x | x ≠ -1}", "(-2, 0)", 0.1));

        questions.Add(Q(T("2.2.2"), "The solution set of x² + x + 3 < 0 is:",
            D.Medium, "Δ = 1 - 12 = -11 < 0, a = 1 > 0. Parabola always above x-axis → never < 0. Solution: ∅.",
            "∅", "R", "(-3, 0)", "{x | x < -1}", 0.1));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 3: FUNCTIONS  (Topics 3.1.x – 3.4.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("3.1.1"), "In the Cartesian coordinate system, point P(-3, -7) is in:",
            D.Easy, "x = -3 < 0 and y = -7 < 0, so the point is in the third quadrant.",
            "Third quadrant", "First quadrant", "Second quadrant", "Fourth quadrant", -1.5));

        questions.Add(Q(T("3.1.1"), "The point symmetric to P(-2, 3) about the y-axis is:",
            D.Easy, "Reflection about y-axis changes sign of x-coordinate: (-(-2), 3) = (2, 3).",
            "(2, 3)", "(-2, -3)", "(2, -3)", "(3, 2)", -1.2));

        questions.Add(Q(T("3.1.1"), "The distance between P(6, -8) and Q(1, 4) is:",
            D.Easy, "d = √[(1-6)² + (4-(-8))²] = √[25 + 144] = √169 = 13.",
            "13", "√119", "12", "15", -0.8));

        questions.Add(Q(T("3.1.2"), "Which function is identical to y = x?",
            D.Medium, "y = ∛(x³) has domain R, range R, and ∛(x³) = x for all x. All three match y = x.",
            "y = ∛(x³)", "y = x²/x", "y = √(x²)", "y = 2x", 0.5));

        questions.Add(Q(T("3.2.1"), "Function y = 2x - 3 on (-∞, +∞) is:",
            D.Easy, "The slope is 2 > 0, so the function is strictly increasing on R.",
            "Increasing", "Decreasing", "Not monotonic", "Cannot determine", -1.0));

        questions.Add(Q(T("3.2.2"), "Function f(x) = -2x³ is:",
            D.Easy, "f(-x) = -2(-x)³ = 2x³ = -(-2x³) = -f(x). So it is odd.",
            "Odd", "Even", "Neither odd nor even", "Both odd and even", -0.8));

        questions.Add(Q(T("3.2.2"), "Function f(x) = x³ - 2 is:",
            D.Medium, "f(-x) = (-x)³ - 2 = -x³ - 2. -f(x) = -x³ + 2. f(-x) ≠ f(x) and f(-x) ≠ -f(x). Neither.",
            "Neither odd nor even", "Odd", "Even", "Both odd and even", 0.2));

        questions.Add(Q(T("3.3.1"), "Which of the following is a power function?",
            D.Easy, "A power function has the form y = xᵃ. Only y = x⁻⁴ fits this form.",
            "y = x⁻⁴", "y = 2x - 1", "y = 2x⁷", "y = x² - 1", -0.5));

        questions.Add(Q(T("3.3.2"), "The domain of y = log₃(x - 2) is:",
            D.Easy, "The argument of the logarithm must be positive: x - 2 > 0, so x > 2. Domain: (2, +∞).",
            "(2, +∞)", "[2, +∞)", "(-∞, 2)", "R", -0.8));

        questions.Add(Q(T("3.3.2"), "If log₄(2x + 3) > log₄(x - 4), then x ∈:",
            D.Hard, "Base 4 > 1, so log is increasing: 2x+3 > x-4 → x > -7. Also 2x+3 > 0 → x > -3/2, and x-4 > 0 → x > 4. Intersection: x > 4.",
            "(4, +∞)", "(-7, +∞)", "(-3/2, +∞)", "(0, +∞)", 1.2));

        questions.Add(Q(T("3.3.1"), "The inequality 3^(x-1) > 3^(2-3x) holds when:",
            D.Medium, "Since base 3 > 1, the exponential is increasing: x - 1 > 2 - 3x → 4x > 3 → x > 3/4.",
            "x > 3/4", "x < 3/4", "x > 1", "x < -7", 0.3));

        questions.Add(Q(T("3.4.1"), "The inverse function of y = 2x + 1 is:",
            D.Easy, "y = 2x + 1 → x = (y-1)/2. Swap: y = (x-1)/2.",
            "y = (x - 1)/2", "y = (x + 1)/2", "y = 2x - 1", "y = x/2 + 1", -0.8));

        questions.Add(Q(T("3.4.2"), "The inverse function of y = log₂ x is:",
            D.Easy, "y = log₂ x → x = 2ʸ. Swap: y = 2ˣ.",
            "y = 2ˣ", "y = x²", "y = log₂ x", "y = eˣ", -0.5));

        questions.Add(Q(T("3.3.1"), "Let a = 0.8^1.7, b = 0.8^0.9, c = 1.2^0.8. Then:",
            D.Hard, "For 0 < base = 0.8 < 1: larger exponent → smaller value: a < b. Also a, b < 1 since 0.8^x < 1 for x > 0. And c > 1 since 1.2 > 1. So a < b < c, meaning c > b > a.",
            "c > b > a", "a > b > c", "b > a > c", "b > c > a", 1.0));

        questions.Add(Q(T("3.2.1"), "The increasing interval of y = sin x is:",
            D.Easy, "y = sin x increases on [-π/2 + 2kπ, π/2 + 2kπ]. For k=0: [-π/2, π/2].",
            "[-π/2, π/2]", "[-π, 0]", "[0, π]", "[π/2, 3π/2]", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 4: TRIGONOMETRIC FUNCTIONS  (Topics 4.1.x – 4.3.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("4.1.1"), "800° is in which quadrant?",
            D.Easy, "800° = 2×360° + 80°. Since 0° < 80° < 90°, it is a first quadrant angle.",
            "First quadrant", "Second quadrant", "Third quadrant", "Fourth quadrant", -1.0));

        questions.Add(Q(T("4.1.1"), "Which angle has the same terminal side as 330°?",
            D.Easy, "330° - 360° = -30°. So -30° has the same terminal side.",
            "-30°", "30°", "150°", "-150°", -0.8));

        questions.Add(Q(T("4.1.1"), "If α is a third quadrant angle, then α/2 is in quadrant:",
            D.Hard, "Third quadrant: 180°+360°k < α < 270°+360°k. So 90°+180°k < α/2 < 135°+180°k. For k=0: QII. For k=1: QIV. Answer: second or fourth quadrant.",
            "Second or fourth quadrant", "First or second quadrant", "Second or third quadrant", "First or third quadrant", 1.5));

        questions.Add(Q(T("4.1.2"), "-335° is in which quadrant?",
            D.Easy, "-335° + 360° = 25°. Since 0° < 25° < 90°, it is a first quadrant angle.",
            "First quadrant", "Second quadrant", "Third quadrant", "Fourth quadrant", -0.8));

        questions.Add(Q(T("4.1.2"), "-300° in radians equals:",
            D.Easy, "-300° × (π/180°) = -5π/3. But we can also write it as +60° = π/3. Actually -300° = -300π/180 = -5π/3.",
            "-5π/3", "4π/3", "5π/3", "7π/6", -0.5));

        questions.Add(Q(T("4.2.1"), "Given P(-3, 4) on the terminal side of α, sin α = ?",
            D.Easy, "r = √(9+16) = 5. sin α = y/r = 4/5.",
            "4/5", "-3/5", "3/5", "-4/5", -0.8));

        questions.Add(Q(T("4.2.1"), "If cos α > 0, then α is in quadrant(s):",
            D.Easy, "cos α = x/r > 0 means x > 0, which happens in quadrants I and IV.",
            "I or IV", "I or II", "II or III", "III or IV", -0.5));

        questions.Add(Q(T("4.2.2"), "cos 1110° equals:",
            D.Medium, "1110° = 3×360° + 30°. cos 30° = √3/2. Actually 1110 = 3×360 + 30. cos 1110° = cos 30° = √3/2. Wait: 3×360 = 1080. 1110-1080=30. cos 30° = √3/2.",
            "√3/2", "-√3/2", "1/2", "-1/2", 0.2));

        questions.Add(Q(T("4.3.1"), "sin²α + cos²α = ?",
            D.Easy, "This is the fundamental Pythagorean identity which always equals 1.",
            "1", "0", "sin 2α", "2", -1.5));

        questions.Add(Q(T("4.3.1"), "Given tan α = 3, find (3sinα - cosα)/(sinα + 2cosα):",
            D.Hard, "Divide numerator and denominator by cosα: (3tanα - 1)/(tanα + 2) = (9 - 1)/(3 + 2) = 8/5.",
            "8/5", "3/5", "2", "1", 1.2));

        questions.Add(Q(T("4.3.2"), "sin(π + α) = ?",
            D.Easy, "By the reduction formula: sin(π + α) = -sin α.",
            "-sin α", "sin α", "cos α", "-cos α", -1.0));

        questions.Add(Q(T("4.3.2"), "cos(-α) = ?",
            D.Easy, "Cosine is an even function: cos(-α) = cos α.",
            "cos α", "-cos α", "sin α", "-sin α", -1.0));

        questions.Add(Q(T("4.2.2"), "The minimal positive period of y = 4sin(-2x + π/3) is:",
            D.Medium, "Period = 2π/|ω| = 2π/|-2| = π.",
            "π", "2π", "π/2", "4π", 0.0));

        questions.Add(Q(T("4.2.2"), "Which function is odd?",
            D.Medium, "f(x) = -sin 2x. f(-x) = -sin(-2x) = sin 2x = -(-sin 2x) = -f(x). Odd.",
            "y = -sin 2x", "y = sin x + 1", "y = sin(x + π/4)", "y = 3sin(5x - π/6)", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 5: INVERSE TRIG FUNCTIONS  (Topics 5.1.x – 5.2.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("5.1.1"), "The range of y = arcsin x is:",
            D.Easy, "By definition, arcsin x returns values in [-π/2, π/2].",
            "[-π/2, π/2]", "[0, π]", "(-π, π)", "[-1, 1]", -1.0));

        questions.Add(Q(T("5.1.1"), "The domain of y = arccos x is:",
            D.Easy, "arccos is defined for x ∈ [-1, 1].",
            "[-1, 1]", "[0, π]", "(-∞, +∞)", "[-π/2, π/2]", -1.0));

        questions.Add(Q(T("5.1.2"), "arcsin(1/2) = ?",
            D.Easy, "sin(π/6) = 1/2, and π/6 ∈ [-π/2, π/2]. So arcsin(1/2) = π/6.",
            "π/6", "π/3", "π/4", "π/2", -1.0));

        questions.Add(Q(T("5.2.1"), "arccos(-√3/2) = ?",
            D.Medium, "cos(5π/6) = -√3/2, and 5π/6 ∈ [0, π]. So arccos(-√3/2) = 5π/6.",
            "5π/6", "π/6", "7π/6", "11π/6", 0.2));

        questions.Add(Q(T("5.2.2"), "sin(arccos(3/5)) = ?",
            D.Medium, "Let θ = arccos(3/5), then cosθ = 3/5 and θ ∈ [0,π]. sinθ = √(1-9/25) = 4/5.",
            "4/5", "3/5", "5/4", "3/4", 0.4));

        questions.Add(Q(T("5.2.2"), "arctan(1) = ?",
            D.Easy, "tan(π/4) = 1, and π/4 ∈ (-π/2, π/2). So arctan(1) = π/4.",
            "π/4", "π/2", "π/3", "π/6", -0.8));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 6: TRIG SUM/DIFFERENCE FORMULAS  (Topics 6.1.x – 6.3.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("6.1.1"), "sin(α + β) = ?",
            D.Easy, "Sum formula: sin(α+β) = sinα·cosβ + cosα·sinβ.",
            "sinα cosβ + cosα sinβ", "sinα sinβ + cosα cosβ",
            "sinα cosβ - cosα sinβ", "cosα cosβ - sinα sinβ", -1.2));

        questions.Add(Q(T("6.1.1"), "cos(α - β) = ?",
            D.Easy, "Difference formula: cos(α-β) = cosα·cosβ + sinα·sinβ.",
            "cosα cosβ + sinα sinβ", "cosα cosβ - sinα sinβ",
            "sinα cosβ + cosα sinβ", "sinα cosβ - cosα sinβ", -1.0));

        questions.Add(Q(T("6.1.2"), "sin 2α = ?",
            D.Easy, "Double angle formula: sin 2α = 2 sinα cosα.",
            "2 sinα cosα", "sin²α + cos²α", "cos²α - sin²α", "2 cos²α - 1", -1.0));

        questions.Add(Q(T("6.1.2"), "cos 2α equals which of the following?",
            D.Easy, "cos 2α = cos²α - sin²α (also = 2cos²α - 1 = 1 - 2sin²α).",
            "cos²α - sin²α", "2 sinα cosα", "sin²α + cos²α", "sin²α - cos²α", -0.8));

        questions.Add(Q(T("6.1.3"), "The half-angle formula: cos²(α/2) = ?",
            D.Medium, "From cos α = 2cos²(α/2) - 1: cos²(α/2) = (1 + cosα)/2.",
            "(1 + cosα)/2", "(1 - cosα)/2", "cosα/2", "(cosα + sinα)/2", 0.2));

        questions.Add(Q(T("6.3.1"), "The general solution of sin x = 1/2 is:",
            D.Medium, "x = π/6 + 2kπ or x = 5π/6 + 2kπ, which can be written as x = (-1)ⁿ·π/6 + nπ, n ∈ Z.",
            "x = (-1)ⁿ·(π/6) + nπ, n ∈ Z", "x = π/6 + 2kπ only",
            "x = π/3 + kπ, k ∈ Z", "x = π/6 + kπ, k ∈ Z", 0.5));

        questions.Add(Q(T("6.2.1"), "sin 75° = sin(45° + 30°) equals:",
            D.Medium, "sin 45° cos 30° + cos 45° sin 30° = (√2/2)(√3/2) + (√2/2)(1/2) = (√6 + √2)/4.",
            "(√6 + √2)/4", "(√6 - √2)/4", "(√3 + 1)/4", "√3/2", 0.3));

        questions.Add(Q(T("6.2.2"), "cos 15° = cos(45° - 30°) equals:",
            D.Medium, "cos 45° cos 30° + sin 45° sin 30° = (√2/2)(√3/2) + (√2/2)(1/2) = (√6 + √2)/4.",
            "(√6 + √2)/4", "(√6 - √2)/4", "(√3 + 1)/4", "√2/2", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 7: SEQUENCES  (Topics 7.1.x – 7.3.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("7.1.1"), "An arithmetic sequence has a₁ = 3 and d = 5. The general term aₙ = ?",
            D.Easy, "aₙ = a₁ + (n-1)d = 3 + (n-1)·5 = 5n - 2.",
            "5n - 2", "5n + 3", "3n + 5", "5n - 3", -0.8));

        questions.Add(Q(T("7.1.2"), "The sum of the first 100 natural numbers (1+2+...+100) is:",
            D.Easy, "S = n(a₁+aₙ)/2 = 100×(1+100)/2 = 5050.",
            "5050", "5000", "5100", "10100", -1.0));

        questions.Add(Q(T("7.1.2"), "In an arithmetic sequence, a₁ = 2, d = 3. The sum S₁₀ = ?",
            D.Medium, "S₁₀ = 10×2 + 10×9/2 × 3 = 20 + 135 = 155.",
            "155", "150", "165", "145", 0.2));

        questions.Add(Q(T("7.2.1"), "A geometric sequence has a₁ = 2 and q = 3. The general term aₙ = ?",
            D.Easy, "aₙ = a₁·qⁿ⁻¹ = 2·3ⁿ⁻¹.",
            "2·3ⁿ⁻¹", "3·2ⁿ⁻¹", "2ⁿ·3", "6ⁿ⁻¹", -0.8));

        questions.Add(Q(T("7.2.2"), "In a geometric sequence with a₁ = 1, q = 2, the sum S₁₀ = ?",
            D.Medium, "S₁₀ = a₁(1-q¹⁰)/(1-q) = 1×(1-1024)/(1-2) = 1023.",
            "1023", "1024", "512", "2046", 0.2));

        questions.Add(Q(T("7.3.1"), "If aₙ₊₁ = 2aₙ + 1 with a₁ = 1, then a₃ = ?",
            D.Medium, "a₂ = 2(1)+1 = 3. a₃ = 2(3)+1 = 7.",
            "7", "5", "9", "15", 0.0));

        questions.Add(Q(T("7.3.2"), "The sum 1/(1·2) + 1/(2·3) + 1/(3·4) + ... + 1/(n(n+1)) = ?",
            D.Hard, "By partial fractions: 1/(k(k+1)) = 1/k - 1/(k+1). Telescoping sum = 1 - 1/(n+1) = n/(n+1).",
            "n/(n+1)", "1/(n+1)", "n/(n-1)", "(n+1)/n", 1.0));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 8: COMPLEX NUMBERS  (Topics 8.1.x – 8.3.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("8.1.1"), "The imaginary unit i satisfies:",
            D.Easy, "By definition, i² = -1.",
            "i² = -1", "i² = 1", "i² = i", "i² = -i", -1.5));

        questions.Add(Q(T("8.1.2"), "(3 + 2i) + (1 - 4i) = ?",
            D.Easy, "Add real parts: 3+1=4. Add imaginary parts: 2+(-4)=-2. Result: 4-2i.",
            "4 - 2i", "4 + 2i", "2 - 2i", "4 - 6i", -1.0));

        questions.Add(Q(T("8.1.2"), "(2 + 3i)(1 - i) = ?",
            D.Medium, "2(1) + 2(-i) + 3i(1) + 3i(-i) = 2 - 2i + 3i - 3i² = 2 + i + 3 = 5 + i.",
            "5 + i", "5 - i", "-1 + 5i", "2 + i", 0.2));

        questions.Add(Q(T("8.2.1"), "The modulus of z = 3 + 4i is:",
            D.Easy, "|z| = √(3² + 4²) = √(9 + 16) = √25 = 5.",
            "5", "7", "√7", "25", -1.0));

        questions.Add(Q(T("8.2.1"), "The conjugate of z = 2 - 5i is:",
            D.Easy, "The conjugate changes the sign of the imaginary part: z̄ = 2 + 5i.",
            "2 + 5i", "-2 + 5i", "-2 - 5i", "5 - 2i", -1.0));

        questions.Add(Q(T("8.2.2"), "The polar form of z = 1 + i is:",
            D.Medium, "|z| = √2, arg(z) = π/4. So z = √2(cos π/4 + i sin π/4).",
            "√2(cos π/4 + i sin π/4)", "2(cos π/4 + i sin π/4)",
            "√2(cos π/3 + i sin π/3)", "(cos π/4 + i sin π/4)", 0.3));

        questions.Add(Q(T("8.3.1"), "By De Moivre's theorem, (cos θ + i sin θ)ⁿ = ?",
            D.Medium, "De Moivre's theorem: (cos θ + i sin θ)ⁿ = cos nθ + i sin nθ.",
            "cos nθ + i sin nθ", "n cos θ + i n sin θ",
            "cosⁿθ + i sinⁿθ", "cos(θ/n) + i sin(θ/n)", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 9: LINES IN PLANE  (Topics 9.1.x – 9.3.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("9.1.1"), "The slope of the line passing through A(1, 2) and B(3, 6) is:",
            D.Easy, "k = (y₂-y₁)/(x₂-x₁) = (6-2)/(3-1) = 4/2 = 2.",
            "2", "4", "1/2", "3", -1.0));

        questions.Add(Q(T("9.1.2"), "The line through (1, 3) with slope 2 has the equation:",
            D.Easy, "Point-slope form: y - 3 = 2(x - 1) → y = 2x + 1.",
            "y = 2x + 1", "y = 2x + 3", "y = 2x - 1", "y = 3x + 1", -0.8));

        questions.Add(Q(T("9.2.1"), "Two lines are perpendicular if their slopes satisfy:",
            D.Easy, "k₁·k₂ = -1 for perpendicular lines.",
            "k₁·k₂ = -1", "k₁·k₂ = 1", "k₁ = k₂", "k₁ + k₂ = 0", -1.0));

        questions.Add(Q(T("9.2.1"), "The line y = 3x - 1 is parallel to:",
            D.Easy, "Parallel lines have equal slopes. y = 3x + 5 has slope 3.",
            "y = 3x + 5", "y = -3x + 5", "y = x/3 + 5", "y = -x/3 + 5", -1.0));

        questions.Add(Q(T("9.2.2"), "The distance from point P(2, 3) to line 3x + 4y - 5 = 0 is:",
            D.Medium, "d = |3(2)+4(3)-5|/√(9+16) = |6+12-5|/5 = 13/5.",
            "13/5", "11/5", "3", "13/7", 0.3));

        questions.Add(Q(T("9.3.1"), "The system x + y = 3, 2x - y = 0 has solution:",
            D.Easy, "From 2nd: y = 2x. Substitute: x + 2x = 3 → x = 1, y = 2.",
            "(1, 2)", "(2, 1)", "(3, 0)", "(0, 3)", -0.5));

        questions.Add(Q(T("9.1.2"), "The general form equation of a line with x-intercept 3 and y-intercept 2 is:",
            D.Medium, "Intercept form: x/3 + y/2 = 1 → 2x + 3y - 6 = 0.",
            "2x + 3y - 6 = 0", "3x + 2y - 6 = 0", "2x + 3y + 6 = 0", "x + y - 5 = 0", 0.2));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 10: CONIC SECTIONS  (Topics 10.1.x – 10.5.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("10.1.1"), "The equation of a circle with center (2, -1) and radius 3 is:",
            D.Easy, "Standard form: (x-h)² + (y-k)² = r² → (x-2)² + (y+1)² = 9.",
            "(x-2)² + (y+1)² = 9", "(x+2)² + (y-1)² = 9",
            "(x-2)² + (y+1)² = 3", "(x-2)² + (y-1)² = 9", -1.0));

        questions.Add(Q(T("10.1.1"), "The center and radius of x² + y² - 4x + 6y - 3 = 0 are:",
            D.Medium, "Complete the square: (x-2)² + (y+3)² = 16. Center (2,-3), radius 4.",
            "Center (2, -3), radius 4", "Center (-2, 3), radius 4",
            "Center (2, -3), radius 16", "Center (4, -6), radius √3", 0.3));

        questions.Add(Q(T("10.2.1"), "An ellipse with equation x²/25 + y²/9 = 1 has semi-major axis a = ?",
            D.Easy, "a² = 25 (larger denominator), so a = 5.",
            "5", "3", "25", "9", -0.8));

        questions.Add(Q(T("10.2.2"), "For the ellipse x²/25 + y²/9 = 1, the eccentricity e = ?",
            D.Medium, "a² = 25, b² = 9, c² = a²-b² = 16, c = 4. e = c/a = 4/5.",
            "4/5", "3/5", "5/4", "3/4", 0.3));

        questions.Add(Q(T("10.3.1"), "A hyperbola with equation x²/16 - y²/9 = 1 has foci on:",
            D.Easy, "The positive term is x², so foci are on the x-axis.",
            "The x-axis", "The y-axis", "The line y = x", "A circle", -0.8));

        questions.Add(Q(T("10.3.2"), "The asymptotes of x²/16 - y²/9 = 1 are:",
            D.Medium, "Asymptotes: y = ±(b/a)x = ±(3/4)x.",
            "y = ±3x/4", "y = ±4x/3", "y = ±3x/16", "y = ±x", 0.3));

        questions.Add(Q(T("10.4.1"), "The focus of the parabola y² = 8x is at:",
            D.Easy, "y² = 4px → 4p = 8 → p = 2. Focus at (2, 0).",
            "(2, 0)", "(0, 2)", "(4, 0)", "(8, 0)", -0.5));

        questions.Add(Q(T("10.4.1"), "The directrix of the parabola y² = 12x is:",
            D.Medium, "4p = 12 → p = 3. Directrix: x = -3.",
            "x = -3", "x = 3", "y = -3", "x = -6", 0.0));

        questions.Add(Q(T("10.5.1"), "If a line y = kx + 1 intersects the circle x² + y² = 1, the number of intersection points depends on:",
            D.Hard, "Substitute: x² + (kx+1)² = 1 → (1+k²)x² + 2kx = 0 → x((1+k²)x + 2k) = 0. Always has x=0 as a solution, giving (0,1). The second root x = -2k/(1+k²). So there are 2 intersection points (they coincide when -2k/(1+k²) = 0, i.e., k = 0, giving 1 point).",
            "The value of k determines whether there is 1 or 2 points",
            "Always 2 points", "Always 1 point", "Always 0 points", 1.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 11: PLANE VECTORS  (Topics 11.1.x – 11.3.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("11.1.1"), "A vector is characterized by:",
            D.Easy, "A vector has both magnitude (length) and direction.",
            "Magnitude and direction", "Magnitude only", "Direction only", "Position and length", -1.5));

        questions.Add(Q(T("11.1.2"), "If a⃗ = (2, 3) and b⃗ = (1, -1), then a⃗ + b⃗ = ?",
            D.Easy, "Add component-wise: (2+1, 3+(-1)) = (3, 2).",
            "(3, 2)", "(1, 4)", "(3, -2)", "(2, 2)", -1.0));

        questions.Add(Q(T("11.1.2"), "If a⃗ = (4, 2) and b⃗ = (1, 3), then a⃗ - b⃗ = ?",
            D.Easy, "Subtract component-wise: (4-1, 2-3) = (3, -1).",
            "(3, -1)", "(5, 5)", "(3, 1)", "(-3, 1)", -1.0));

        questions.Add(Q(T("11.1.2"), "3 × (2, -1) = ?",
            D.Easy, "Scalar multiplication: (3×2, 3×(-1)) = (6, -3).",
            "(6, -3)", "(5, 2)", "(6, 3)", "(2, -3)", -1.2));

        questions.Add(Q(T("11.2.1"), "The dot product of a⃗ = (3, 4) and b⃗ = (2, -1) is:",
            D.Easy, "a⃗ · b⃗ = 3×2 + 4×(-1) = 6 - 4 = 2.",
            "2", "10", "-2", "6", -0.8));

        questions.Add(Q(T("11.2.1"), "Two vectors a⃗ = (2, 1) and b⃗ = (-1, k) are perpendicular when k = ?",
            D.Medium, "a⃗ · b⃗ = 0: 2(-1) + 1(k) = 0 → k = 2.",
            "2", "-2", "1/2", "-1/2", 0.0));

        questions.Add(Q(T("11.2.2"), "The magnitude of vector a⃗ = (3, 4) is:",
            D.Easy, "|a⃗| = √(3² + 4²) = √(9 + 16) = √25 = 5.",
            "5", "7", "√7", "25", -1.0));

        questions.Add(Q(T("11.3.1"), "The vector from A(1, 2) to B(4, 6) is:",
            D.Easy, "AB⃗ = (4-1, 6-2) = (3, 4).",
            "(3, 4)", "(5, 8)", "(4, 6)", "(-3, -4)", -1.0));

        questions.Add(Q(T("11.2.1"), "If |a⃗| = 3, |b⃗| = 4, and the angle between them is 60°, then a⃗ · b⃗ = ?",
            D.Medium, "a⃗ · b⃗ = |a⃗||b⃗|cos θ = 3×4×cos 60° = 12×(1/2) = 6.",
            "6", "12", "3", "10", 0.2));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 12: SPACE VECTORS  (Topics 12.1.x – 12.2.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("12.1.1"), "The magnitude of vector a⃗ = (1, 2, 2) is:",
            D.Easy, "|a⃗| = √(1² + 2² + 2²) = √(1 + 4 + 4) = √9 = 3.",
            "3", "√5", "9", "√8", -0.8));

        questions.Add(Q(T("12.1.1"), "If a⃗ = (1, 0, -1) and b⃗ = (2, 1, 0), then a⃗ + b⃗ = ?",
            D.Easy, "(1+2, 0+1, -1+0) = (3, 1, -1).",
            "(3, 1, -1)", "(3, 1, 1)", "(1, 1, -1)", "(2, 0, 0)", -1.0));

        questions.Add(Q(T("12.1.2"), "The cross product a⃗ × b⃗ where a⃗ = (1, 0, 0) and b⃗ = (0, 1, 0) is:",
            D.Medium, "i×j = k. So (1,0,0) × (0,1,0) = (0,0,1).",
            "(0, 0, 1)", "(0, 0, -1)", "(1, 1, 0)", "(0, 0, 0)", 0.0));

        questions.Add(Q(T("12.2.1"), "The dot product a⃗ · b⃗ where a⃗ = (1, 2, 3) and b⃗ = (4, -1, 2) is:",
            D.Easy, "1×4 + 2×(-1) + 3×2 = 4 - 2 + 6 = 8.",
            "8", "10", "6", "12", -0.5));

        questions.Add(Q(T("12.2.2"), "The distance between points P(1, 2, 3) and Q(4, 6, 3) is:",
            D.Easy, "d = √[(4-1)² + (6-2)² + (3-3)²] = √[9 + 16 + 0] = √25 = 5.",
            "5", "√26", "7", "√50", -0.5));

        questions.Add(Q(T("12.2.2"), "The angle between a⃗ = (1, 1, 0) and b⃗ = (0, 1, 1) is:",
            D.Medium, "cos θ = a⃗·b⃗/(|a⃗||b⃗|) = (0+1+0)/(√2·√2) = 1/2. θ = π/3.",
            "π/3", "π/4", "π/6", "π/2", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 13: SPACE PLANES & LINES  (Topics 13.1.x – 13.2.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("13.1.1"), "The equation of a plane with normal vector n⃗ = (1, 2, 3) passing through (0, 0, 0) is:",
            D.Easy, "The plane equation: 1(x-0) + 2(y-0) + 3(z-0) = 0 → x + 2y + 3z = 0.",
            "x + 2y + 3z = 0", "x + 2y + 3z = 1", "x - 2y - 3z = 0", "2x + y + 3z = 0", -0.8));

        questions.Add(Q(T("13.1.1"), "Two planes are parallel if their normal vectors are:",
            D.Easy, "Parallel planes have proportional (parallel) normal vectors.",
            "Parallel (proportional)", "Perpendicular", "Equal in magnitude", "Opposite", -1.0));

        questions.Add(Q(T("13.1.2"), "A line perpendicular to a plane is perpendicular to:",
            D.Easy, "By definition, it is perpendicular to every line that lies in the plane and passes through the foot.",
            "Every line in the plane through the foot", "Only horizontal lines", "Only two specific lines", "No lines in the plane", -1.2));

        questions.Add(Q(T("13.2.1"), "A dihedral angle is the angle between:",
            D.Medium, "A dihedral angle is formed by two half-planes sharing a common edge.",
            "Two half-planes with a common edge", "Two intersecting lines",
            "A line and a plane", "Two parallel planes", 0.0));

        questions.Add(Q(T("13.2.2"), "The distance from point P(1, 1, 1) to the plane x + y + z = 0 is:",
            D.Medium, "d = |1+1+1-0|/√(1²+1²+1²) = 3/√3 = √3.",
            "√3", "3", "1", "3/√3", 0.3));

        questions.Add(Q(T("13.1.2"), "Two planes x + 2y + 3z = 1 and 2x + 4y + 6z = 5 are:",
            D.Medium, "Normal vectors: (1,2,3) and (2,4,6) are proportional (ratio 1:2), so planes are parallel.",
            "Parallel", "Perpendicular", "Intersecting at a line", "Identical", 0.2));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 14: LIMITS  (Topics 14.1.x – 14.3.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("14.1.1"), "lim(n→∞) 1/n = ?",
            D.Easy, "As n → ∞, 1/n → 0.",
            "0", "1", "∞", "Does not exist", -1.5));

        questions.Add(Q(T("14.1.1"), "lim(n→∞) (3n+1)/(n+2) = ?",
            D.Medium, "Divide by n: (3 + 1/n)/(1 + 2/n) → 3/1 = 3 as n → ∞.",
            "3", "1", "∞", "3/2", 0.0));

        questions.Add(Q(T("14.1.2"), "lim(x→2) (x² - 4)/(x - 2) = ?",
            D.Easy, "(x²-4)/(x-2) = (x-2)(x+2)/(x-2) = x+2. As x→2: 2+2 = 4.",
            "4", "0", "2", "∞", -0.8));

        questions.Add(Q(T("14.1.2"), "lim(x→0) sin x / x = ?",
            D.Medium, "This is one of the two important limits. lim(x→0) sin x / x = 1.",
            "1", "0", "π", "∞", 0.0));

        questions.Add(Q(T("14.2.1"), "lim(x→∞) (1 + 1/x)ˣ = ?",
            D.Medium, "This is the second important limit, which equals e ≈ 2.718...",
            "e", "1", "∞", "0", 0.2));

        questions.Add(Q(T("14.2.2"), "A function f(x) is continuous at x = a if:",
            D.Medium, "f is continuous at a if lim(x→a) f(x) = f(a), meaning limit exists and equals the function value.",
            "lim(x→a) f(x) = f(a)", "f(a) exists", "lim(x→a) f(x) exists",
            "f is defined on (a-δ, a+δ)", 0.3));

        questions.Add(Q(T("14.3.1"), "If f(x) ≤ g(x) ≤ h(x) and lim f(x) = lim h(x) = L, then lim g(x) = ?",
            D.Medium, "By the Squeeze Theorem, lim g(x) = L.",
            "L", "0", "Does not exist", "(f + h)/2", 0.2));

        questions.Add(Q(T("14.1.2"), "lim(x→0) (1 - cos x)/x² = ?",
            D.Hard, "Using L'Hôpital or the identity 1-cosx = 2sin²(x/2): lim 2sin²(x/2)/x² = 2·(1/4) = 1/2.",
            "1/2", "1", "0", "2", 1.0));

        questions.Add(Q(T("14.2.1"), "lim(x→∞) (2x² + 3x)/(5x² - 1) = ?",
            D.Medium, "Divide by x²: (2 + 3/x)/(5 - 1/x²) → 2/5.",
            "2/5", "∞", "0", "2", 0.0));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 15: DERIVATIVES  (Topics 15.1.x – 15.2.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("15.1.1"), "The derivative of f(x) = x³ is:",
            D.Easy, "f'(x) = 3x².",
            "3x²", "x²", "3x", "x³/3", -1.0));

        questions.Add(Q(T("15.1.1"), "If f(x) = 5x² + 3x - 1, then f'(x) = ?",
            D.Easy, "f'(x) = 10x + 3.",
            "10x + 3", "10x + 1", "5x + 3", "10x² + 3", -0.8));

        questions.Add(Q(T("15.1.2"), "The derivative f'(a) geometrically represents:",
            D.Easy, "f'(a) is the slope of the tangent line to f(x) at x = a.",
            "The slope of the tangent line at x = a", "The area under the curve",
            "The y-intercept", "The maximum value of f", -1.0));

        questions.Add(Q(T("15.2.1"), "The derivative of f(x) = eˣ is:",
            D.Easy, "The exponential function eˣ is its own derivative: (eˣ)' = eˣ.",
            "eˣ", "xeˣ⁻¹", "eˣ⁻¹", "ln x", -1.0));

        questions.Add(Q(T("15.2.1"), "The derivative of f(x) = ln x is:",
            D.Easy, "(ln x)' = 1/x.",
            "1/x", "ln x", "x", "1/(x ln x)", -0.8));

        questions.Add(Q(T("15.2.2"), "If f(x) = sin(3x), then f'(x) = ?",
            D.Medium, "By the chain rule: f'(x) = cos(3x) · 3 = 3cos(3x).",
            "3cos(3x)", "cos(3x)", "-3cos(3x)", "3sin(3x)", 0.0));

        questions.Add(Q(T("15.2.2"), "If f(x) = e^(2x+1), then f'(x) = ?",
            D.Medium, "By the chain rule: f'(x) = e^(2x+1) · 2 = 2e^(2x+1).",
            "2e^(2x+1)", "e^(2x+1)", "(2x+1)e^(2x)", "2xe^(2x+1)", 0.0));

        questions.Add(Q(T("15.2.3"), "(sin x)' = ?",
            D.Easy, "The derivative of sin x is cos x.",
            "cos x", "-cos x", "sin x", "-sin x", -1.0));

        questions.Add(Q(T("15.2.3"), "(cos x)' = ?",
            D.Easy, "The derivative of cos x is -sin x.",
            "-sin x", "sin x", "cos x", "-cos x", -1.0));

        questions.Add(Q(T("15.2.3"), "(tan x)' = ?",
            D.Medium, "The derivative of tan x is sec²x = 1/cos²x.",
            "1/cos²x", "1/sin²x", "-1/cos²x", "cos x/sin x", 0.0));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 16: APPLICATIONS OF DERIVATIVES  (Topics 16.1.x – 16.3.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("16.1.1"), "f(x) = x³ - 3x is increasing on the interval:",
            D.Medium, "f'(x) = 3x² - 3 = 3(x²-1) > 0 when x < -1 or x > 1. Increasing on (-∞,-1) and (1,+∞).",
            "(-∞, -1) ∪ (1, +∞)", "(-1, 1)", "(0, +∞)", "(-∞, 0)", 0.3));

        questions.Add(Q(T("16.1.1"), "f(x) = x² - 4x + 5 is decreasing on:",
            D.Easy, "f'(x) = 2x - 4 < 0 when x < 2. Decreasing on (-∞, 2).",
            "(-∞, 2)", "(2, +∞)", "(-∞, 4)", "(0, +∞)", -0.5));

        questions.Add(Q(T("16.1.2"), "The function f(x) = x³ - 3x has a local maximum at:",
            D.Medium, "f'(x) = 3x² - 3 = 0 → x = ±1. f''(x) = 6x. f''(-1) = -6 < 0 → local max at x = -1.",
            "x = -1", "x = 1", "x = 0", "x = 3", 0.3));

        questions.Add(Q(T("16.2.1"), "A rectangle has perimeter 20. The maximum area is:",
            D.Medium, "Let side = x. Perimeter: 2x + 2y = 20 → y = 10-x. Area A = x(10-x). A'= 10-2x = 0 → x = 5. A = 25.",
            "25", "20", "100", "50", 0.2));

        questions.Add(Q(T("16.2.2"), "The second derivative test says f has a local minimum at x = a if:",
            D.Medium, "f'(a) = 0 and f''(a) > 0 implies a local minimum.",
            "f'(a) = 0 and f''(a) > 0", "f'(a) = 0 and f''(a) < 0",
            "f'(a) > 0 and f''(a) = 0", "f'(a) < 0 and f''(a) = 0", 0.3));

        questions.Add(Q(T("16.3.1"), "An inflection point of f(x) occurs where:",
            D.Medium, "An inflection point occurs where f''(x) changes sign (concavity changes).",
            "f''(x) changes sign", "f'(x) = 0", "f(x) = 0", "f'(x) changes sign", 0.4));

        questions.Add(Q(T("16.1.2"), "f(x) = eˣ - x has a minimum at:",
            D.Medium, "f'(x) = eˣ - 1 = 0 → x = 0. f''(0) = e⁰ = 1 > 0 → local min at x = 0.",
            "x = 0", "x = 1", "x = -1", "x = e", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 17: PERMUTATIONS & COMBINATIONS  (Topics 17.1.x – 17.2.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("17.1.1"), "From city A to B there are 3 bus routes and 2 train routes. How many ways to travel from A to B?",
            D.Easy, "By the addition principle: 3 + 2 = 5 ways.",
            "5", "6", "3", "2", -1.2));

        questions.Add(Q(T("17.1.1"), "From A to B there are 3 routes, from B to C there are 4 routes. Ways to travel A→B→C:",
            D.Easy, "By the multiplication principle: 3 × 4 = 12 ways.",
            "12", "7", "24", "10", -1.0));

        questions.Add(Q(T("17.1.2"), "P(5, 3) = ?",
            D.Easy, "P(5,3) = 5!/(5-3)! = 5×4×3 = 60.",
            "60", "10", "120", "20", -0.5));

        questions.Add(Q(T("17.1.3"), "C(6, 2) = ?",
            D.Easy, "C(6,2) = 6!/(2!·4!) = (6×5)/(2×1) = 15.",
            "15", "30", "12", "6", -0.5));

        questions.Add(Q(T("17.1.3"), "C(n, 0) = ?",
            D.Easy, "C(n,0) = 1 for any n, since there is exactly one way to choose nothing.",
            "1", "0", "n", "n!", -1.2));

        questions.Add(Q(T("17.2.1"), "The coefficient of x² in the expansion of (1 + x)⁵ is:",
            D.Medium, "By the binomial theorem: C(5,2)·1³·x² = 10x². Coefficient is 10.",
            "10", "5", "20", "1", 0.0));

        questions.Add(Q(T("17.2.1"), "The expansion of (a + b)⁴ has how many terms?",
            D.Easy, "(a+b)ⁿ has n+1 terms. Here n=4, so 5 terms.",
            "5", "4", "16", "8", -0.8));

        questions.Add(Q(T("17.2.2"), "In Pascal's triangle, the sum of the entries in row n is:",
            D.Medium, "The sum of C(n,0) + C(n,1) + ... + C(n,n) = 2ⁿ.",
            "2ⁿ", "n!", "nⁿ", "n²", 0.0));

        questions.Add(Q(T("17.1.2"), "How many 3-digit numbers can be formed from {1, 2, 3, 4, 5} without repetition?",
            D.Medium, "P(5,3) = 5×4×3 = 60.",
            "60", "125", "120", "10", 0.2));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 18: RANDOM EVENTS & PROBABILITY  (Topics 18.1.x – 18.3.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("18.1.1"), "The probability of an impossible event is:",
            D.Easy, "P(impossible event) = 0.",
            "0", "1", "-1", "1/2", -1.5));

        questions.Add(Q(T("18.1.1"), "If P(A) = 0.3 and P(B) = 0.5 where A, B are mutually exclusive, then P(A ∪ B) = ?",
            D.Easy, "For mutually exclusive events: P(A ∪ B) = P(A) + P(B) = 0.3 + 0.5 = 0.8.",
            "0.8", "0.15", "0.2", "1.0", -0.8));

        questions.Add(Q(T("18.1.2"), "A die is rolled once. The probability of getting an even number is:",
            D.Easy, "Even numbers: {2, 4, 6}. P = 3/6 = 1/2.",
            "1/2", "1/3", "1/6", "2/3", -1.2));

        questions.Add(Q(T("18.1.2"), "Two dice are rolled. The probability that the sum is 7 is:",
            D.Medium, "Favorable: (1,6),(2,5),(3,4),(4,3),(5,2),(6,1) = 6 outcomes. Total: 36. P = 6/36 = 1/6.",
            "1/6", "1/12", "7/36", "1/7", 0.2));

        questions.Add(Q(T("18.2.1"), "P(A|B) is defined as:",
            D.Medium, "Conditional probability: P(A|B) = P(A ∩ B)/P(B).",
            "P(A ∩ B)/P(B)", "P(A ∪ B)/P(B)", "P(A)·P(B)", "P(A) + P(B)", 0.0));

        questions.Add(Q(T("18.2.2"), "Events A and B are independent if:",
            D.Medium, "A and B independent: P(A ∩ B) = P(A)·P(B).",
            "P(A ∩ B) = P(A)·P(B)", "P(A ∪ B) = P(A) + P(B)",
            "P(A|B) = P(B)", "A ∩ B = ∅", 0.0));

        questions.Add(Q(T("18.2.2"), "If A and B are independent with P(A) = 0.4 and P(B) = 0.5, then P(A ∩ B) = ?",
            D.Easy, "P(A ∩ B) = P(A)·P(B) = 0.4 × 0.5 = 0.2.",
            "0.2", "0.9", "0.45", "0.1", -0.5));

        questions.Add(Q(T("18.3.1"), "If P(A) = 0.6, then P(Ā) (complement) = ?",
            D.Easy, "P(Ā) = 1 - P(A) = 1 - 0.6 = 0.4.",
            "0.4", "0.6", "0.3", "-0.6", -1.0));

        questions.Add(Q(T("18.1.2"), "From 5 cards numbered 1-5, two are drawn. P(both are odd) = ?",
            D.Medium, "Odd cards: {1,3,5} = 3 cards. C(3,2)/C(5,2) = 3/10.",
            "3/10", "2/5", "1/5", "3/5", 0.3));

        questions.Add(Q(T("18.2.1"), "In Bayes' theorem: P(Bᵢ|A) = ?",
            D.Hard, "P(Bᵢ|A) = P(A|Bᵢ)P(Bᵢ) / Σ P(A|Bⱼ)P(Bⱼ).",
            "P(A|Bᵢ)P(Bᵢ) / ΣP(A|Bⱼ)P(Bⱼ)", "P(A ∩ Bᵢ) + P(Bᵢ)",
            "P(Bᵢ)/P(A)", "P(A)·P(Bᵢ)", 1.2));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 19: RANDOM VARIABLES  (Topics 19.1.x – 19.3.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("19.1.1"), "A discrete random variable X takes values 0, 1, 2 with P(X=0)=0.3, P(X=1)=0.5. Then P(X=2) = ?",
            D.Easy, "Probabilities sum to 1: P(X=2) = 1 - 0.3 - 0.5 = 0.2.",
            "0.2", "0.8", "0.5", "0.3", -0.8));

        questions.Add(Q(T("19.1.2"), "For a probability distribution table, the sum of all probabilities must equal:",
            D.Easy, "By the axioms of probability, Σ P(X = xᵢ) = 1.",
            "1", "0", "The number of outcomes", "∞", -1.2));

        questions.Add(Q(T("19.2.1"), "If X takes values 1, 2, 3 each with probability 1/3, then E(X) = ?",
            D.Easy, "E(X) = 1·(1/3) + 2·(1/3) + 3·(1/3) = 6/3 = 2.",
            "2", "1", "3", "6", -0.5));

        questions.Add(Q(T("19.2.1"), "E(aX + b) = ?",
            D.Medium, "By linearity of expectation: E(aX + b) = aE(X) + b.",
            "aE(X) + b", "a²E(X) + b", "aE(X) + b²", "E(X) + ab", 0.0));

        questions.Add(Q(T("19.2.2"), "Var(X) is defined as:",
            D.Medium, "Var(X) = E[(X - E(X))²] = E(X²) - [E(X)]².",
            "E(X²) - [E(X)]²", "E(X) - [E(X²)]", "[E(X)]² - E(X²)", "E(X²) + [E(X)]²", 0.2));

        questions.Add(Q(T("19.2.2"), "Var(aX + b) = ?",
            D.Medium, "Var(aX + b) = a²·Var(X). Adding a constant doesn't change variance.",
            "a²Var(X)", "aVar(X) + b", "a²Var(X) + b²", "a²Var(X) + b", 0.3));

        questions.Add(Q(T("19.3.1"), "If X ~ B(10, 0.3), then E(X) = ?",
            D.Medium, "For binomial distribution: E(X) = np = 10 × 0.3 = 3.",
            "3", "7", "0.3", "30", 0.0));

        questions.Add(Q(T("19.3.1"), "If X ~ B(n, p), then Var(X) = ?",
            D.Medium, "For binomial distribution: Var(X) = np(1-p).",
            "np(1-p)", "np", "np²", "n²p(1-p)", 0.2));

        questions.Add(Q(T("19.3.2"), "The standard normal distribution N(0, 1) has mean and variance:",
            D.Easy, "By definition, N(0,1) has mean μ = 0 and variance σ² = 1.",
            "μ = 0, σ² = 1", "μ = 1, σ² = 0", "μ = 0, σ² = 0", "μ = 1, σ² = 1", -1.0));

        questions.Add(Q(T("19.3.2"), "If X ~ N(μ, σ²), then P(μ - σ < X < μ + σ) ≈ ?",
            D.Medium, "By the empirical rule (68-95-99.7 rule): P ≈ 68.26%.",
            "68.26%", "95.44%", "50%", "99.74%", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 20: STATISTICS  (Topics 20.1.x – 20.3.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("20.1.1"), "Which is NOT a sampling method?",
            D.Easy, "Common sampling methods include simple random, systematic, and stratified. 'Preferential' is not a standard method.",
            "Preferential sampling", "Simple random sampling", "Stratified sampling", "Systematic sampling", -0.8));

        questions.Add(Q(T("20.1.2"), "A frequency distribution histogram shows:",
            D.Easy, "It shows the frequency (or relative frequency) of data in each class interval.",
            "Frequency of data in each interval", "The mean of the data",
            "Individual data points", "The standard deviation", -0.8));

        questions.Add(Q(T("20.2.1"), "The mean of {2, 4, 6, 8, 10} is:",
            D.Easy, "Mean = (2+4+6+8+10)/5 = 30/5 = 6.",
            "6", "5", "8", "30", -1.2));

        questions.Add(Q(T("20.2.1"), "The median of {1, 3, 5, 7, 9, 11} is:",
            D.Easy, "6 values, median = average of 3rd and 4th: (5+7)/2 = 6.",
            "6", "5", "7", "5.5", -0.8));

        questions.Add(Q(T("20.2.1"), "The mode of {1, 2, 2, 3, 3, 3, 4} is:",
            D.Easy, "3 appears most frequently (3 times). Mode = 3.",
            "3", "2", "1", "4", -1.0));

        questions.Add(Q(T("20.2.2"), "The variance of a data set measures:",
            D.Easy, "Variance measures the average squared deviation from the mean.",
            "Average squared deviation from the mean", "The most frequent value",
            "The middle value", "The difference between max and min", -0.5));

        questions.Add(Q(T("20.2.2"), "The range of {3, 7, 2, 9, 5} is:",
            D.Easy, "Range = max - min = 9 - 2 = 7.",
            "7", "5", "9", "6", -1.0));

        questions.Add(Q(T("20.3.1"), "In linear regression ŷ = bx + a, b represents:",
            D.Easy, "b is the regression coefficient (slope), indicating the rate of change of y per unit change in x.",
            "The slope (rate of change)", "The y-intercept", "The correlation coefficient", "The variance", -0.5));

        questions.Add(Q(T("20.3.2"), "The correlation coefficient r satisfies:",
            D.Medium, "-1 ≤ r ≤ 1. When |r| is close to 1, the linear correlation is strong.",
            "-1 ≤ r ≤ 1", "0 ≤ r ≤ 1", "r > 0 always", "r can be any real number", 0.0));

        questions.Add(Q(T("20.3.2"), "If r = -0.95, the correlation between X and Y is:",
            D.Easy, "|r| = 0.95 close to 1, indicating strong correlation. Negative sign means negative correlation.",
            "Strong negative linear correlation", "Weak positive correlation",
            "No correlation", "Strong positive correlation", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // EXPANSION BATCH — +200 questions across all 20 chapters
        // ═══════════════════════════════════════════════════════════════

        // ── Ch1 Sets (+9) ────────────────────────────────────────────

        questions.Add(Q(T("1.1.1"), "Which of the following numbers belongs to the set of natural numbers N?",
            D.Easy, "N = {0, 1, 2, 3, ...}. 0 is a natural number.",
            "0", "-1", "1/2", "√2", -1.5));

        questions.Add(Q(T("1.1.2"), "Which number belongs to Q but not to Z?",
            D.Easy, "Q is rational numbers, Z is integers. 3/2 is rational but not an integer.",
            "3/2", "5", "-3", "0", -1.0));

        questions.Add(Q(T("1.1.3"), "Which statement about number sets is correct?",
            D.Medium, "Z⁺ ⊂ Z ⊂ Q ⊂ R is the correct hierarchy of number sets.",
            "Z⁺ ⊂ Z ⊂ Q ⊂ R", "N ⊂ Z⁺ ⊂ Q", "Q ⊂ Z ⊂ N", "R ⊂ Q ⊂ Z", 0.2));

        questions.Add(Q(T("1.2.1"), "How many subsets does the set {1, 3, 5, 7} have?",
            D.Easy, "A set with n elements has 2ⁿ subsets. Here n = 4, so 2⁴ = 16.",
            "16", "8", "15", "4", -0.8));

        questions.Add(Q(T("1.2.2"), "If A ⊆ B and B ⊆ A, then:",
            D.Easy, "By the definition of set equality, A ⊆ B and B ⊆ A implies A = B.",
            "A = B", "A ⊂ B", "A = ∅", "B = ∅", -1.0));

        questions.Add(Q(T("1.2.3"), "The empty set ∅ is a subset of:",
            D.Easy, "By definition, the empty set is a subset of every set.",
            "Every set", "Only ∅ itself", "No set", "Only finite sets", -1.2));

        questions.Add(Q(T("1.4.1"), "A = {x | -3 < x ≤ 4}, B = {x | -5 ≤ x < 2}. Then A ∩ B = ?",
            D.Medium, "A ∩ B = {x | x ∈ A and x ∈ B} = {x | -3 < x < 2}.",
            "(-3, 2)", "[-5, 4]", "(-5, 2)", "[-3, 4]", 0.3));

        questions.Add(Q(T("1.4.2"), "U = R, A = (-4, 2), B = [-1, 5]. Then A ∪ B = ?",
            D.Medium, "A ∪ B combines both intervals: (-4, 5].",
            "(-4, 5]", "[-1, 2)", "(-4, 2)", "[-1, 5]", 0.2));

        questions.Add(Q(T("1.4.3"), "If A ∩ B = A, which must be true?",
            D.Medium, "A ∩ B = A means every element of A is in B, i.e., A ⊆ B.",
            "A ⊆ B", "B ⊆ A", "A = ∅", "B = ∅", 0.3));

        // ── Ch2 Inequalities (+8) ───────────────────────────────────

        questions.Add(Q(T("2.1.1"), "If -2 < a < 3, then the range of -3a is:",
            D.Easy, "Multiply by -3 (negative): -3(3) < -3a < -3(-2) → -9 < -3a < 6.",
            "-9 < -3a < 6", "-6 < -3a < 9", "6 < -3a < 9", "-6 < -3a < -9", -0.5));

        questions.Add(Q(T("2.2.1"), "Solve: 2x² - 7x + 3 > 0",
            D.Medium, "Roots: x = (7 ± 5)/4 → x = 1/2 and x = 3. For > 0: x < 1/2 or x > 3.",
            "(-∞, 1/2) ∪ (3, +∞)", "(1/2, 3)", "(-∞, 3) ∪ (1/2, +∞)", "[1/2, 3]", 0.4));

        questions.Add(Q(T("2.2.2"), "The solution set of 2x² + 4x + 3 ≥ 0 is:",
            D.Medium, "Δ = 16 - 24 = -8 < 0. Since a = 2 > 0, the parabola is always above the x-axis. Solution: R.",
            "R", "∅", "{x | x ≥ 0}", "(-∞, -1]", 0.1));

        questions.Add(Q(T("2.2.2"), "Solve: x² - 4x + 4 ≤ 0",
            D.Medium, "(x - 2)² ≤ 0. A square is always ≥ 0, so the only solution is x = 2.",
            "{2}", "∅", "[0, 4]", "R", 0.3));

        questions.Add(Q(T("2.3.1"), "Solve: (x + 4)/(5x + 3) < 0",
            D.Medium, "Critical points: x = -4, x = -3/5. Product negative: -4 < x < -3/5.",
            "(-4, -3/5)", "(-∞, -4) ∪ (-3/5, +∞)", "(-4, 0)", "(-3/5, +∞)", 0.4));

        questions.Add(Q(T("2.3.2"), "Solve: (x - 5)/(x + 3) ≥ 0",
            D.Medium, "Product ≥ 0: x ≤ -3 or x ≥ 5. Exclude x = -3 (denominator = 0). So x < -3 or x ≥ 5.",
            "(-∞, -3) ∪ [5, +∞)", "(-∞, -3] ∪ [5, +∞)", "[-3, 5]", "(-3, 5)", 0.5));

        questions.Add(Q(T("2.1.3"), "Given -3 < a < 0, 2 < b < 4, then the range of a + 2b is:",
            D.Medium, "4 < 2b < 8. Adding: -3 + 4 < a + 2b < 0 + 8 → 1 < a + 2b < 8.",
            "(1, 8)", "(-3, 8)", "(1, 4)", "(-1, 12)", 0.3));

        questions.Add(Q(T("2.1.2"), "The solution set of -6x² - x + 2 ≤ 0 is:",
            D.Hard, "Multiply by -1: 6x² + x - 2 ≥ 0. Roots: x = 1/2, x = -2/3. For ≥ 0: x ≤ -2/3 or x ≥ 1/2.",
            "(-∞, -2/3] ∪ [1/2, +∞)", "[-2/3, 1/2]", "(-∞, -1/2] ∪ [2/3, +∞)", "R", 0.8));

        // ── Ch3 Functions (+15) ─────────────────────────────────────

        questions.Add(Q(T("3.1.1"), "The distance from point P(6, -8) to the x-axis is:",
            D.Easy, "Distance from P(x, y) to x-axis is |y| = |-8| = 8.",
            "8", "6", "10", "14", -1.2));

        questions.Add(Q(T("3.1.1"), "Point P(x, y) reflected about the origin becomes:",
            D.Easy, "Reflection about the origin: P'(-x, -y).",
            "(-x, -y)", "(x, -y)", "(-x, y)", "(y, x)", -1.0));

        questions.Add(Q(T("3.1.2"), "The domain of f(x) = √(x - 1) + 1/(x - 4) is:",
            D.Medium, "Need x - 1 ≥ 0 and x ≠ 4. Domain: {x | x ≥ 1, x ≠ 4}.",
            "{x | x ≥ 1, x ≠ 4}", "{x | x > 4}", "{x | x ≥ 1}", "[1, 4) ∪ (4, +∞)", 0.3));

        questions.Add(Q(T("3.2.1"), "The function y = x² + 1 is increasing on:",
            D.Easy, "Vertex at (0, 1). The parabola increases on (0, +∞).",
            "(0, +∞)", "(-∞, 0)", "(-∞, +∞)", "(-1, 1)", -0.5));

        questions.Add(Q(T("3.2.1"), "The function y = 1/x on (-∞, 0) is:",
            D.Medium, "For x₁ < x₂ < 0: 1/x₁ - 1/x₂ = (x₂ - x₁)/(x₁x₂) > 0. So 1/x₁ > 1/x₂. Decreasing.",
            "Decreasing", "Increasing", "Not monotonic", "Constant", 0.2));

        questions.Add(Q(T("3.2.2"), "The function f(x) = x² + 1 is:",
            D.Easy, "f(-x) = (-x)² + 1 = x² + 1 = f(x). Even function.",
            "Even", "Odd", "Neither odd nor even", "Both odd and even", -0.8));

        questions.Add(Q(T("3.3.1"), "If base 0 < a < 1, then y = aˣ is:",
            D.Easy, "When 0 < a < 1, the exponential function y = aˣ is decreasing on R.",
            "Decreasing on R", "Increasing on R", "Increasing on (0, +∞)", "Not monotonic", -0.5));

        questions.Add(Q(T("3.3.2"), "log₂ 8 = ?",
            D.Easy, "2³ = 8, so log₂ 8 = 3.",
            "3", "4", "2", "8", -1.5));

        questions.Add(Q(T("3.3.2"), "log₃ 1 = ?",
            D.Easy, "For any base a > 0, a ≠ 1: logₐ 1 = 0 since a⁰ = 1.",
            "0", "1", "3", "-1", -1.5));

        questions.Add(Q(T("3.3.2"), "The logarithmic function y = log₂ x passes through the point:",
            D.Easy, "All logₐ x functions pass through (1, 0) since logₐ 1 = 0.",
            "(1, 0)", "(0, 1)", "(2, 1)", "(0, 0)", -0.8));

        questions.Add(Q(T("3.3.1"), "The domain of y = 3^(2/(x - 1)) is:",
            D.Medium, "The exponent 2/(x-1) requires x ≠ 1. Domain: {x | x ≠ 1}.",
            "{x | x ≠ 1}", "(1, +∞)", "R", "{x | x > 0}", 0.2));

        questions.Add(Q(T("3.4.1"), "The inverse function of y = (2x - 1)/(x + 3), x ≠ -3, is:",
            D.Hard, "y(x+3) = 2x-1 → x(2-y) = 3y+1 → x = (3y+1)/(2-y). Swap: y = (3x+1)/(2-x).",
            "y = (3x + 1)/(2 - x)", "y = (2x - 1)/(x + 3)", "y = (x + 3)/(2x - 1)", "y = (2 - x)/(3x + 1)", 1.0));

        questions.Add(Q(T("3.3.2"), "If log₀.₅(2x - 3) ≥ 0, then x ∈:",
            D.Hard, "Base 0.5 < 1, log is decreasing. log₀.₅(2x-3) ≥ log₀.₅ 1 → 2x-3 ≤ 1 → x ≤ 2. Also 2x-3 > 0 → x > 3/2.",
            "(3/2, 2]", "[2, +∞)", "(0, 3/2)", "[3/2, +∞)", 1.0));

        questions.Add(Q(T("3.4.2"), "The inverse function of y = 2ˣ is:",
            D.Easy, "y = 2ˣ → x = log₂ y. Swap: y = log₂ x.",
            "y = log₂ x", "y = x²", "y = 2ˣ", "y = ln x", -0.5));

        questions.Add(Q(T("3.3.1"), "The inequality (1/4)^(2x+3) > (1/4)^(x-4) holds when:",
            D.Medium, "Base 1/4 < 1 → decreasing: 2x+3 < x-4 → x < -7.",
            "x < -7", "x > -7", "x < 4", "x > 4", 0.4));

        // ── Ch4 Trigonometric Functions (+11) ────────────────────────

        questions.Add(Q(T("4.1.1"), "-840° is in which quadrant?",
            D.Medium, "-840° = -2×360° - 120°. -120° + 360° = 240°. 180° < 240° < 270° → third quadrant.",
            "Third quadrant", "First quadrant", "Second quadrant", "Fourth quadrant", 0.3));

        questions.Add(Q(T("4.1.2"), "If α = 6π/7, its terminal side lies in:",
            D.Easy, "π/2 < 6π/7 < π → second quadrant.",
            "Second quadrant", "First quadrant", "Third quadrant", "Fourth quadrant", -0.5));

        questions.Add(Q(T("4.2.1"), "If sin α cos α > 0, α is in quadrant(s):",
            D.Medium, "sin and cos same sign: both + in QI, both - in QIII.",
            "I or III", "I or II", "II or IV", "III or IV", 0.2));

        questions.Add(Q(T("4.2.2"), "The range of y = 5 sin x + 2 is:",
            D.Easy, "-1 ≤ sin x ≤ 1 → -5+2 ≤ 5 sin x + 2 ≤ 5+2. Range: [-3, 7].",
            "[-3, 7]", "[-5, 5]", "[2, 7]", "[-3, 5]", -0.5));

        questions.Add(Q(T("4.3.1"), "sin(π - α) = ?",
            D.Easy, "Reduction formula: sin(π - α) = sin α.",
            "sin α", "-sin α", "cos α", "-cos α", -1.0));

        questions.Add(Q(T("4.3.2"), "cos(π/2 - α) = ?",
            D.Easy, "Co-function identity: cos(π/2 - α) = sin α.",
            "sin α", "cos α", "-sin α", "-cos α", -1.0));

        questions.Add(Q(T("4.3.2"), "sin(π/2 + α) = ?",
            D.Medium, "sin(π/2 + α) = cos α.",
            "cos α", "sin α", "-cos α", "-sin α", 0.0));

        questions.Add(Q(T("4.2.1"), "Given P(2, -2√3) on terminal side of α, then tan α = ?",
            D.Medium, "tan α = y/x = (-2√3)/2 = -√3.",
            "-√3", "√3", "-1/√3", "2", 0.0));

        questions.Add(Q(T("4.2.2"), "The minimal positive period of y = tan 3x is:",
            D.Easy, "Period of tan(ωx) = π/|ω| = π/3.",
            "π/3", "π", "2π/3", "3π", -0.5));

        questions.Add(Q(T("4.3.1"), "Given sin α = 1/3, find cos(π/2 - α):",
            D.Easy, "cos(π/2 - α) = sin α = 1/3.",
            "1/3", "2√2/3", "-1/3", "√(8/9)", -0.5));

        questions.Add(Q(T("4.2.1"), "If tan α < 0, α is in which quadrant(s)?",
            D.Easy, "tan α < 0 when sin and cos have different signs: QII or QIV.",
            "II or IV", "I or III", "I or II", "III or IV", -0.5));

        // ── Ch5 Inverse Trig Functions (+6) ─────────────────────────

        questions.Add(Q(T("5.1.1"), "arcsin(-√2/2) = ?",
            D.Easy, "sin(-π/4) = -√2/2, and -π/4 ∈ [-π/2, π/2]. So arcsin(-√2/2) = -π/4.",
            "-π/4", "π/4", "-3π/4", "3π/4", -0.8));

        questions.Add(Q(T("5.1.2"), "arccos(0) = ?",
            D.Easy, "cos(π/2) = 0, and π/2 ∈ [0, π]. So arccos(0) = π/2.",
            "π/2", "0", "π", "π/4", -1.0));

        questions.Add(Q(T("5.2.1"), "arctan(-√3) = ?",
            D.Easy, "tan(-π/3) = -√3, and -π/3 ∈ (-π/2, π/2).",
            "-π/3", "π/3", "-π/6", "2π/3", -0.5));

        questions.Add(Q(T("5.2.2"), "arccot(1) = ?",
            D.Easy, "cot(π/4) = 1, and π/4 ∈ (0, π).",
            "π/4", "π/2", "π/3", "0", -0.8));

        questions.Add(Q(T("5.1.2"), "cos(arcsin(4/5)) = ?",
            D.Medium, "Let θ = arcsin(4/5), sin θ = 4/5, θ ∈ [0, π/2]. cos θ = √(1 - 16/25) = 3/5.",
            "3/5", "4/5", "5/3", "4/3", 0.3));

        questions.Add(Q(T("5.2.1"), "arctan(√3/3) = ?",
            D.Easy, "tan(π/6) = √3/3, and π/6 ∈ (-π/2, π/2).",
            "π/6", "π/3", "π/4", "π/2", -0.8));

        // ── Ch6 Trig Sum/Difference Formulas (+10) ──────────────────

        questions.Add(Q(T("6.1.1"), "tan(α + β) = ?",
            D.Medium, "Sum formula: tan(α+β) = (tanα + tanβ)/(1 - tanα·tanβ).",
            "(tanα + tanβ)/(1 - tanα tanβ)", "tanα + tanβ",
            "(tanα - tanβ)/(1 + tanα tanβ)", "tanα tanβ/(tanα + tanβ)", 0.2));

        questions.Add(Q(T("6.1.2"), "tan 2α = ?",
            D.Medium, "Double angle: tan 2α = 2tanα/(1 - tan²α).",
            "2tanα/(1 - tan²α)", "2tanα", "tan²α - 1", "tanα/(1 - tanα)", 0.2));

        questions.Add(Q(T("6.1.3"), "sin²(α/2) = ?",
            D.Medium, "From cos α = 1 - 2sin²(α/2): sin²(α/2) = (1 - cosα)/2.",
            "(1 - cosα)/2", "(1 + cosα)/2", "sinα/2", "(cosα - 1)/2", 0.2));

        questions.Add(Q(T("6.2.1"), "cos 105° = cos(60° + 45°) equals:",
            D.Medium, "cos60°cos45° - sin60°sin45° = (1/2)(√2/2) - (√3/2)(√2/2) = (√2 - √6)/4.",
            "(√2 - √6)/4", "(√6 - √2)/4", "(√6 + √2)/4", "(√2 + √6)/4", 0.3));

        questions.Add(Q(T("6.2.1"), "tan 15° = tan(45° - 30°) equals:",
            D.Medium, "(1 - √3/3)/(1 + √3/3) = (3-√3)/(3+√3) = 2 - √3.",
            "2 - √3", "2 + √3", "√3 - 1", "1 - √3/3", 0.4));

        questions.Add(Q(T("6.1.2"), "Given sin α = 3/5, α in QII, find sin 2α:",
            D.Medium, "cos α = -4/5 (QII). sin 2α = 2·(3/5)·(-4/5) = -24/25.",
            "-24/25", "24/25", "-12/25", "12/25", 0.4));

        questions.Add(Q(T("6.1.2"), "Given cos α = -2/5, find cos 2α:",
            D.Medium, "cos 2α = 2cos²α - 1 = 2(4/25) - 1 = -17/25.",
            "-17/25", "17/25", "-21/25", "8/25", 0.3));

        questions.Add(Q(T("6.2.2"), "sin 105° cos 105° = ?",
            D.Medium, "(1/2)sin 210° = (1/2)(-1/2) = -1/4.",
            "-1/4", "1/4", "√3/4", "-√3/4", 0.3));

        questions.Add(Q(T("6.3.1"), "The general solution of cos x = -1/2 is:",
            D.Medium, "x = ±2π/3 + 2kπ, k ∈ Z.",
            "x = ±2π/3 + 2kπ, k ∈ Z", "x = π/3 + 2kπ only",
            "x = 2π/3 + kπ, k ∈ Z", "x = π/6 + 2kπ, k ∈ Z", 0.5));

        questions.Add(Q(T("6.1.1"), "The maximum value of y = 4 sin x - 3 cos x is:",
            D.Medium, "Max = √(a² + b²) = √(16 + 9) = 5.",
            "5", "7", "4", "√7", 0.2));

        // ── Ch7 Sequences (+11) ─────────────────────────────────────

        questions.Add(Q(T("7.1.1"), "In an arithmetic sequence, a₁ = 5, d = -2. Then a₁₀ = ?",
            D.Easy, "a₁₀ = a₁ + 9d = 5 + 9(-2) = -13.",
            "-13", "-15", "-11", "23", -0.5));

        questions.Add(Q(T("7.1.1"), "If a₃ = 7 and a₇ = 15 in an arithmetic sequence, d = ?",
            D.Medium, "a₇ - a₃ = 4d = 15 - 7 = 8, so d = 2.",
            "2", "4", "8", "1", 0.0));

        questions.Add(Q(T("7.1.2"), "Arithmetic sequence: a₁ = 1, aₙ = 100, n = 10. Then S₁₀ = ?",
            D.Easy, "S₁₀ = n(a₁ + aₙ)/2 = 10(1 + 100)/2 = 505.",
            "505", "550", "1010", "450", -0.5));

        questions.Add(Q(T("7.2.1"), "In a geometric sequence, a₁ = 3, q = -2. Then a₄ = ?",
            D.Easy, "a₄ = a₁·q³ = 3·(-8) = -24.",
            "-24", "24", "-12", "48", -0.5));

        questions.Add(Q(T("7.2.1"), "In a geometric sequence, a₂ = 6, a₅ = 162. The common ratio q = ?",
            D.Medium, "a₅ = a₂·q³ → q³ = 27 → q = 3.",
            "3", "27", "9", "√3", 0.2));

        questions.Add(Q(T("7.2.2"), "Sum of infinite geometric series: a₁ = 4, q = 1/2:",
            D.Medium, "S = a₁/(1-q) = 4/(1/2) = 8.",
            "8", "4", "∞", "2", 0.2));

        questions.Add(Q(T("7.1.1"), "The general term of 2, 5, 8, 11, ... is:",
            D.Easy, "Arithmetic: a₁ = 2, d = 3. aₙ = 2 + (n-1)·3 = 3n - 1.",
            "3n - 1", "3n + 2", "2n + 3", "3n", -0.8));

        questions.Add(Q(T("7.3.1"), "If a₁ = 2, aₙ₊₁ = 3aₙ, what type of sequence is {aₙ}?",
            D.Easy, "aₙ₊₁/aₙ = 3 (constant ratio) → geometric with q = 3.",
            "Geometric with q = 3", "Arithmetic with d = 3", "Neither", "Geometric with q = 2", -0.5));

        questions.Add(Q(T("7.1.2"), "Arithmetic sequence: S₅ = 25, S₁₀ = 100. Then S₁₅ = ?",
            D.Hard, "S₅, S₁₀-S₅, S₁₅-S₁₀ form AP. S₁₀-S₅ = 75, d' = 50. S₁₅-S₁₀ = 125. S₁₅ = 225.",
            "225", "200", "250", "175", 1.2));

        questions.Add(Q(T("7.2.2"), "Geometric sequence: a₁ = 1, q = -1. Then S₆ = ?",
            D.Easy, "1-1+1-1+1-1 = 0.",
            "0", "1", "-1", "6", -0.8));

        questions.Add(Q(T("7.3.2"), "Σₖ₌₁ⁿ k = ?",
            D.Easy, "Sum of first n natural numbers: n(n+1)/2.",
            "n(n+1)/2", "n²/2", "n(n-1)/2", "(n+1)²/2", -0.8));

        // ── Ch8 Complex Numbers (+7) ────────────────────────────────

        questions.Add(Q(T("8.1.1"), "i⁴ = ?",
            D.Easy, "i¹ = i, i² = -1, i³ = -i, i⁴ = 1. Pattern repeats every 4.",
            "1", "-1", "i", "-i", -1.0));

        questions.Add(Q(T("8.1.1"), "i⁷ = ?",
            D.Easy, "i⁷ = i⁴⁺³ = 1·(-i) = -i.",
            "-i", "i", "1", "-1", -0.5));

        questions.Add(Q(T("8.1.2"), "(1 + i)² = ?",
            D.Easy, "1 + 2i + i² = 1 + 2i - 1 = 2i.",
            "2i", "2", "2 + 2i", "0", -0.5));

        questions.Add(Q(T("8.2.1"), "(2 + 3i)/(1 - i) = ?",
            D.Hard, "Multiply by conjugate: (2+3i)(1+i)/((1)²+(1)²) = (-1+5i)/2.",
            "(-1 + 5i)/2", "(5 + i)/2", "(2 + 3i)/2", "(1 - 5i)/2", 0.8));

        questions.Add(Q(T("8.1.2"), "(3 - i) - (1 + 2i) = ?",
            D.Easy, "(3-1) + (-1-2)i = 2 - 3i.",
            "2 - 3i", "2 + i", "4 - 3i", "2 - i", -1.0));

        questions.Add(Q(T("8.2.2"), "If z = a + bi and |z| = 0, then:",
            D.Easy, "√(a² + b²) = 0 implies a = 0 and b = 0, so z = 0.",
            "z = 0", "z is purely imaginary", "a = b", "z = 1", -0.8));

        questions.Add(Q(T("8.3.1"), "The argument of z = -1 + i is:",
            D.Medium, "In QII: θ = π - arctan(1) = π - π/4 = 3π/4.",
            "3π/4", "π/4", "5π/4", "-π/4", 0.2));

        // ── Ch9 Lines in Plane (+11) ────────────────────────────────

        questions.Add(Q(T("9.1.1"), "A vertical line x = 3 has slope:",
            D.Easy, "A vertical line has undefined slope.",
            "Undefined", "0", "∞", "3", -1.0));

        questions.Add(Q(T("9.1.1"), "A horizontal line y = 5 has slope:",
            D.Easy, "A horizontal line has slope 0.",
            "0", "Undefined", "5", "1", -1.2));

        questions.Add(Q(T("9.1.2"), "The line 2x + 3y - 6 = 0 has y-intercept:",
            D.Easy, "Set x = 0: 3y = 6 → y = 2.",
            "2", "3", "-2", "6", -0.8));

        questions.Add(Q(T("9.1.2"), "The slope-intercept form of 4x - 2y + 8 = 0 is:",
            D.Easy, "2y = 4x + 8 → y = 2x + 4.",
            "y = 2x + 4", "y = 4x + 8", "y = -2x + 4", "y = 2x - 4", -0.5));

        questions.Add(Q(T("9.2.1"), "Two lines with slopes k₁ = 2 and k₂ = 2 are:",
            D.Easy, "Equal slopes means parallel (or coincident).",
            "Parallel", "Perpendicular", "Intersecting at 45°", "Coincident", -1.0));

        questions.Add(Q(T("9.2.1"), "A line perpendicular to y = 3x + 1 has slope:",
            D.Easy, "k₁·k₂ = -1 → k₂ = -1/3.",
            "-1/3", "3", "1/3", "-3", -0.5));

        questions.Add(Q(T("9.2.2"), "The midpoint of A(2, 4) and B(6, 8) is:",
            D.Easy, "M = ((2+6)/2, (4+8)/2) = (4, 6).",
            "(4, 6)", "(8, 12)", "(3, 5)", "(4, 4)", -0.8));

        questions.Add(Q(T("9.3.1"), "The lines x + y = 2 and x - y = 0 intersect at:",
            D.Easy, "Add: 2x = 2 → x = 1, y = 1.",
            "(1, 1)", "(2, 0)", "(0, 2)", "(1, 2)", -0.5));

        questions.Add(Q(T("9.1.2"), "Line through (2, -1) and (2, 5) has equation:",
            D.Easy, "Both points have x = 2 → vertical line x = 2.",
            "x = 2", "y = 2", "y = 3x - 7", "x + y = 7", -0.8));

        questions.Add(Q(T("9.2.2"), "The distance from origin (0, 0) to line 3x + 4y - 10 = 0 is:",
            D.Easy, "d = |0 + 0 - 10|/√(9+16) = 10/5 = 2.",
            "2", "10", "5", "10/7", -0.5));

        questions.Add(Q(T("9.1.1"), "The inclination angle of y = x is:",
            D.Easy, "Slope k = 1, tan α = 1, α = 45°.",
            "45°", "90°", "30°", "60°", -0.8));

        // ── Ch10 Conic Sections (+16) ───────────────────────────────

        questions.Add(Q(T("10.1.1"), "x² + y² = 4 represents a circle with:",
            D.Easy, "Center (0,0), radius √4 = 2.",
            "Center (0,0), radius 2", "Center (0,0), radius 4",
            "Center (2,2), radius 2", "Center (0,0), radius √2", -1.0));

        questions.Add(Q(T("10.1.1"), "A circle with center (0, 3) and radius 5 has equation:",
            D.Easy, "x² + (y - 3)² = 25.",
            "x² + (y - 3)² = 25", "x² + (y + 3)² = 25",
            "(x - 3)² + y² = 25", "x² + (y - 3)² = 5", -0.8));

        questions.Add(Q(T("10.1.1"), "Does point (1, 2) lie on x² + y² = 5?",
            D.Easy, "1² + 2² = 5. Yes, it satisfies the equation.",
            "Yes, on the circle", "No, inside", "No, outside", "Cannot determine", -1.2));

        questions.Add(Q(T("10.2.1"), "The foci of x²/25 + y²/9 = 1 are at:",
            D.Medium, "a² = 25, b² = 9, c² = 16, c = 4. Foci: (±4, 0).",
            "(±4, 0)", "(0, ±4)", "(±5, 0)", "(±3, 0)", 0.0));

        questions.Add(Q(T("10.2.1"), "An ellipse x²/a² + y²/b² = 1 (a > b > 0) has foci on:",
            D.Easy, "When a² > b², foci lie on the x-axis.",
            "The x-axis", "The y-axis", "The line y = x", "Both axes", -0.5));

        questions.Add(Q(T("10.2.2"), "The sum of distances from any point on an ellipse to its foci equals:",
            D.Easy, "By definition, the sum equals 2a.",
            "2a", "2b", "2c", "a + b", -0.5));

        questions.Add(Q(T("10.3.1"), "Hyperbola x²/9 - y²/4 = 1 has c = ?",
            D.Easy, "c² = a² + b² = 9 + 4 = 13, c = √13.",
            "√13", "√5", "13", "5", -0.5));

        questions.Add(Q(T("10.3.1"), "The foci of x²/9 - y²/4 = 1 are at:",
            D.Medium, "c = √13. Foci on x-axis: (±√13, 0).",
            "(±√13, 0)", "(0, ±√13)", "(±3, 0)", "(±5, 0)", 0.0));

        questions.Add(Q(T("10.3.2"), "The eccentricity of x²/9 - y²/4 = 1 is:",
            D.Medium, "a² = 9, c² = 13. e = c/a = √13/3.",
            "√13/3", "2/3", "3/√13", "13/9", 0.3));

        questions.Add(Q(T("10.4.1"), "The parabola x² = 4y has focus at:",
            D.Easy, "x² = 4py → p = 1. Opens upward → focus at (0, 1).",
            "(0, 1)", "(1, 0)", "(0, 4)", "(0, -1)", -0.5));

        questions.Add(Q(T("10.4.1"), "The parabola y² = -16x opens:",
            D.Easy, "y² = -4px → opens to the left.",
            "To the left", "To the right", "Upward", "Downward", -0.5));

        questions.Add(Q(T("10.2.2"), "For x²/16 + y²/12 = 1, the eccentricity e = ?",
            D.Medium, "a² = 16, b² = 12, c² = 4, c = 2. e = 2/4 = 1/2.",
            "1/2", "2", "√3/2", "3/4", 0.2));

        questions.Add(Q(T("10.1.1"), "x² + y² + Dx + Ey + F = 0 is a circle when:",
            D.Medium, "It represents a circle when D² + E² - 4F > 0.",
            "D² + E² - 4F > 0", "D + E > F", "F > 0", "D² + E² > 0", 0.4));

        questions.Add(Q(T("10.4.1"), "The directrix of x² = 8y is:",
            D.Medium, "x² = 4py → p = 2. Opens upward → directrix: y = -2.",
            "y = -2", "y = 2", "x = -2", "x = 2", 0.0));

        questions.Add(Q(T("10.5.1"), "A tangent line to a circle at point P is perpendicular to:",
            D.Easy, "The tangent is perpendicular to the radius at the point of tangency.",
            "The radius at P", "The diameter", "The x-axis", "The y-axis", -1.0));

        questions.Add(Q(T("10.2.1"), "Which equation represents an ellipse?",
            D.Easy, "x²/9 + y²/4 = 1 is the standard ellipse form.",
            "x²/9 + y²/4 = 1", "x² + y² = 9", "x²/9 - y²/4 = 1", "y = x²", -0.8));

        // ── Ch11 Plane Vectors (+11) ────────────────────────────────

        questions.Add(Q(T("11.1.1"), "The zero vector 0⃗ has:",
            D.Easy, "The zero vector has magnitude 0 but no definite direction.",
            "Magnitude 0, no definite direction", "Magnitude 0, direction along x-axis",
            "Magnitude 1, no direction", "No magnitude, no direction", -1.0));

        questions.Add(Q(T("11.1.2"), "If a⃗ = (2, 3) then |a⃗| = ?",
            D.Easy, "|a⃗| = √(4 + 9) = √13.",
            "√13", "5", "√5", "13", -0.5));

        questions.Add(Q(T("11.1.2"), "Two vectors are parallel if:",
            D.Medium, "Parallel vectors: x₁y₂ - x₂y₁ = 0.",
            "x₁y₂ - x₂y₁ = 0", "x₁x₂ + y₁y₂ = 0", "|a⃗| = |b⃗|", "a⃗ + b⃗ = 0⃗", 0.2));

        questions.Add(Q(T("11.2.1"), "a⃗ · b⃗ = 0 implies the vectors are:",
            D.Easy, "Zero dot product means perpendicular.",
            "Perpendicular", "Parallel", "Equal", "Opposite", -0.8));

        questions.Add(Q(T("11.2.1"), "The angle between a⃗ = (1, 0) and b⃗ = (0, 1) is:",
            D.Easy, "a⃗ · b⃗ = 0. cos θ = 0 → θ = π/2.",
            "π/2", "0", "π", "π/4", -1.0));

        questions.Add(Q(T("11.2.2"), "The projection of a⃗ = (3, 4) onto b⃗ = (1, 0) is:",
            D.Medium, "proj = (a⃗ · b⃗)/|b⃗| = 3/1 = 3.",
            "3", "4", "5", "0", 0.0));

        questions.Add(Q(T("11.3.1"), "In parallelogram ABCD, AB⃗ + AD⃗ = ?",
            D.Medium, "AB⃗ + AD⃗ = AC⃗ (diagonal rule).",
            "AC⃗", "BD⃗", "CB⃗", "DA⃗", 0.0));

        questions.Add(Q(T("11.1.1"), "A unit vector has magnitude:",
            D.Easy, "By definition, a unit vector has magnitude 1.",
            "1", "0", "√2", "Any value", -1.5));

        questions.Add(Q(T("11.1.2"), "The unit vector in the direction of a⃗ = (3, 4) is:",
            D.Medium, "|a⃗| = 5. Unit vector = (3/5, 4/5).",
            "(3/5, 4/5)", "(3, 4)", "(1, 1)", "(4/5, 3/5)", 0.2));

        questions.Add(Q(T("11.2.2"), "a⃗ = (2, -1), b⃗ = (3, 6). cos θ = ?",
            D.Medium, "a⃗ · b⃗ = 6 - 6 = 0. cos θ = 0.",
            "0", "1", "-1", "1/2", 0.0));

        questions.Add(Q(T("11.2.1"), "If a⃗ = (1, 2), then 2a⃗ - (3, 1) = ?",
            D.Easy, "(2,4) - (3,1) = (-1, 3).",
            "(-1, 3)", "(5, 5)", "(-1, -3)", "(1, 3)", -0.5));

        // ── Ch12 Space Vectors (+9) ─────────────────────────────────

        questions.Add(Q(T("12.1.1"), "a⃗ = (2, -1, 3), b⃗ = (1, 2, -1). Then a⃗ · b⃗ = ?",
            D.Easy, "2(1) + (-1)(2) + 3(-1) = 2 - 2 - 3 = -3.",
            "-3", "3", "0", "8", -0.5));

        questions.Add(Q(T("12.1.1"), "The magnitude of a⃗ = (2, 3, 6) is:",
            D.Easy, "|a⃗| = √(4 + 9 + 36) = √49 = 7.",
            "7", "√49", "49", "11", -0.8));

        questions.Add(Q(T("12.1.2"), "a⃗ × b⃗ where a⃗ = (1,0,0), b⃗ = (0,0,1) is:",
            D.Medium, "i × k = -j. Result: (0, -1, 0).",
            "(0, -1, 0)", "(0, 1, 0)", "(0, 0, 0)", "(1, 0, 1)", 0.2));

        questions.Add(Q(T("12.1.1"), "2(1, -1, 0) + (0, 2, 3) = ?",
            D.Easy, "(2, -2, 0) + (0, 2, 3) = (2, 0, 3).",
            "(2, 0, 3)", "(2, 0, 0)", "(2, -2, 3)", "(1, 1, 3)", -0.8));

        questions.Add(Q(T("12.2.1"), "Vectors (1, 2, 3) and (2, 4, 6) are:",
            D.Easy, "(2,4,6) = 2·(1,2,3). They are parallel.",
            "Parallel", "Perpendicular", "At 45°", "At 60°", -0.8));

        questions.Add(Q(T("12.2.2"), "The midpoint of P(1, 0, 3) and Q(3, 4, 1) is:",
            D.Easy, "M = (2, 2, 2).",
            "(2, 2, 2)", "(4, 4, 4)", "(1, 2, 1)", "(2, 0, 2)", -0.8));

        questions.Add(Q(T("12.1.2"), "a⃗ × a⃗ = ?",
            D.Easy, "Cross product of any vector with itself is the zero vector.",
            "(0, 0, 0)", "a⃗", "|a⃗|²", "2a⃗", -1.0));

        questions.Add(Q(T("12.2.1"), "If a⃗ · b⃗ = 0 for 3D vectors, then:",
            D.Easy, "Zero dot product → perpendicular.",
            "a⃗ ⊥ b⃗", "a⃗ ∥ b⃗", "a⃗ = b⃗", "|a⃗| = 0", -0.8));

        questions.Add(Q(T("12.2.2"), "Distance from (0,0,0) to (1,2,2) is:",
            D.Easy, "d = √(1+4+4) = 3.",
            "3", "√6", "5", "√8", -0.8));

        // ── Ch13 Space Planes & Lines (+12) ─────────────────────────

        questions.Add(Q(T("13.1.1"), "The general equation of a plane is:",
            D.Easy, "Ax + By + Cz + D = 0, where (A,B,C) is the normal vector.",
            "Ax + By + Cz + D = 0", "y = mx + b", "x² + y² + z² = r²", "x/a + y/b = 1", -1.0));

        questions.Add(Q(T("13.1.1"), "The normal vector of 3x - 2y + z = 5 is:",
            D.Easy, "Normal = coefficients of x, y, z: (3, -2, 1).",
            "(3, -2, 1)", "(3, 2, 1)", "(-3, 2, -1)", "(5, 0, 0)", -0.8));

        questions.Add(Q(T("13.1.2"), "Two planes are perpendicular if their normals satisfy:",
            D.Easy, "n⃗₁ · n⃗₂ = 0.",
            "n⃗₁ · n⃗₂ = 0", "n⃗₁ × n⃗₂ = 0⃗", "n⃗₁ = n⃗₂", "|n⃗₁| = |n⃗₂|", -0.5));

        questions.Add(Q(T("13.1.1"), "The plane through (1,0,0), (0,1,0), (0,0,1) has equation:",
            D.Medium, "Intercept form: x/1 + y/1 + z/1 = 1 → x + y + z = 1.",
            "x + y + z = 1", "x + y + z = 0", "x - y + z = 1", "x + y - z = 0", 0.2));

        questions.Add(Q(T("13.2.1"), "The angle between two intersecting planes is called:",
            D.Easy, "A dihedral angle.",
            "Dihedral angle", "Included angle", "Reflex angle", "Solid angle", -1.0));

        questions.Add(Q(T("13.2.2"), "Distance from (0,0,0) to plane 2x + 2y + z - 3 = 0:",
            D.Medium, "d = |0+0+0-3|/√(4+4+1) = 3/3 = 1.",
            "1", "3", "1/3", "√3", 0.0));

        questions.Add(Q(T("13.1.2"), "Two non-parallel planes intersect in:",
            D.Easy, "They intersect in a line.",
            "A line", "A point", "A plane", "Empty set", -1.0));

        questions.Add(Q(T("13.1.1"), "Three non-collinear points determine:",
            D.Easy, "They determine a unique plane.",
            "A unique plane", "A line", "Infinitely many planes", "Nothing", -1.0));

        questions.Add(Q(T("13.2.1"), "The angle between planes x + y = 0 and y + z = 0 is:",
            D.Medium, "n₁=(1,1,0), n₂=(0,1,1). cos θ = 1/(√2·√2) = 1/2. θ = π/3.",
            "π/3", "π/4", "π/2", "π/6", 0.3));

        questions.Add(Q(T("13.2.2"), "Distance between parallel planes x+y+z=1 and x+y+z=4:",
            D.Medium, "d = |4-1|/√3 = √3.",
            "√3", "3", "1", "3√3", 0.2));

        questions.Add(Q(T("13.1.2"), "Planes with normals (1,0,0) and (0,1,0) are:",
            D.Easy, "n₁ · n₂ = 0 → perpendicular.",
            "Perpendicular", "Parallel", "Identical", "At 45°", -0.8));

        questions.Add(Q(T("13.1.1"), "The plane z = 0 is:",
            D.Easy, "z = 0 defines the xy-plane.",
            "The xy-plane", "The xz-plane", "The yz-plane", "The origin", -1.2));

        // ── Ch14 Limits (+11) ───────────────────────────────────────

        questions.Add(Q(T("14.1.1"), "lim(n→∞) (2n - 1)/n = ?",
            D.Easy, "Divide by n: 2 - 1/n → 2.",
            "2", "1", "∞", "0", -0.8));

        questions.Add(Q(T("14.1.1"), "lim(n→∞) 3/n² = ?",
            D.Easy, "3/n² → 0.",
            "0", "3", "∞", "1", -1.0));

        questions.Add(Q(T("14.1.2"), "lim(x→3) (x² - 9)/(x - 3) = ?",
            D.Easy, "(x-3)(x+3)/(x-3) = x+3 → 6.",
            "6", "0", "3", "∞", -0.5));

        questions.Add(Q(T("14.2.1"), "lim(x→∞) (5x³ - 2x)/(3x³ + 1) = ?",
            D.Medium, "Same degree: 5/3.",
            "5/3", "∞", "0", "5", 0.0));

        questions.Add(Q(T("14.2.1"), "lim(x→∞) (3x + 1)/(x² + 2) = ?",
            D.Easy, "Numerator degree < denominator degree → 0.",
            "0", "3", "∞", "1/2", -0.5));

        questions.Add(Q(T("14.2.2"), "A discontinuity at x = a means:",
            D.Medium, "f is not continuous: lim f(x) ≠ f(a) or limit doesn't exist.",
            "lim(x→a) f(x) ≠ f(a) or limit doesn't exist", "f(a) is defined",
            "The derivative exists at a", "f is bounded near a", 0.2));

        questions.Add(Q(T("14.3.1"), "lim(x→0) tan x / x = ?",
            D.Medium, "lim (sin x / x) · (1/cos x) = 1·1 = 1.",
            "1", "0", "∞", "π", 0.0));

        questions.Add(Q(T("14.1.2"), "lim(x→1) (x³ - 1)/(x - 1) = ?",
            D.Medium, "(x-1)(x²+x+1)/(x-1) = x²+x+1 → 3.",
            "3", "1", "0", "∞", 0.0));

        questions.Add(Q(T("14.1.1"), "A sequence converges if:",
            D.Medium, "Its terms approach a finite value as n → ∞.",
            "Its terms approach a finite value", "Its terms are increasing",
            "All terms are positive", "It has infinitely many terms", 0.2));

        questions.Add(Q(T("14.2.2"), "If f is continuous on [a,b] and f(a)·f(b) < 0, by IVT:",
            D.Medium, "∃ c ∈ (a,b) such that f(c) = 0.",
            "∃ c ∈ (a,b): f(c) = 0", "f has a maximum on [a,b]",
            "f is differentiable on (a,b)", "f(a) = f(b)", 0.3));

        questions.Add(Q(T("14.3.1"), "lim(x→0) (eˣ - 1)/x = ?",
            D.Medium, "Standard limit = 1 (derivative of eˣ at 0).",
            "1", "e", "0", "∞", 0.2));

        // ── Ch15 Derivatives (+8) ───────────────────────────────────

        questions.Add(Q(T("15.1.1"), "The derivative of a constant f(x) = c is:",
            D.Easy, "Derivative of a constant is 0.",
            "0", "c", "1", "cx", -1.5));

        questions.Add(Q(T("15.1.1"), "If f(x) = xⁿ, then f'(x) = ?",
            D.Easy, "Power rule: nxⁿ⁻¹.",
            "nxⁿ⁻¹", "xⁿ⁻¹", "nxⁿ", "(n-1)xⁿ", -1.0));

        questions.Add(Q(T("15.2.1"), "The derivative of f(x) = aˣ (a > 0, a ≠ 1) is:",
            D.Medium, "(aˣ)' = aˣ ln a.",
            "aˣ ln a", "xaˣ⁻¹", "aˣ/ln a", "aˣ", 0.2));

        questions.Add(Q(T("15.2.2"), "If f(x) = ln(2x + 1), then f'(x) = ?",
            D.Medium, "Chain rule: 1/(2x+1) · 2 = 2/(2x+1).",
            "2/(2x + 1)", "1/(2x + 1)", "ln 2", "2 ln(2x + 1)", 0.2));

        questions.Add(Q(T("15.1.2"), "The product rule: (fg)' = ?",
            D.Easy, "(fg)' = f'g + fg'.",
            "f'g + fg'", "f'g'", "f'g - fg'", "(f + g)'", -0.8));

        questions.Add(Q(T("15.1.2"), "The quotient rule: (f/g)' = ?",
            D.Medium, "(f'g - fg')/g².",
            "(f'g - fg')/g²", "(f'g + fg')/g²", "f'/g'", "f'g - fg'", 0.2));

        questions.Add(Q(T("15.2.3"), "If f(x) = x² sin x, then f'(x) = ?",
            D.Hard, "Product rule: 2x sin x + x² cos x.",
            "2x sin x + x² cos x", "2x cos x", "x² cos x", "2x sin x - x² cos x", 0.8));

        questions.Add(Q(T("15.2.2"), "If f(x) = (3x + 1)⁵, then f'(x) = ?",
            D.Medium, "Chain rule: 5(3x+1)⁴ · 3 = 15(3x+1)⁴.",
            "15(3x + 1)⁴", "5(3x + 1)⁴", "(3x + 1)⁵", "15(3x + 1)⁵", 0.3));

        // ── Ch16 Applications of Derivatives (+8) ───────────────────

        questions.Add(Q(T("16.1.1"), "f(x) = x³ is increasing on:",
            D.Easy, "f'(x) = 3x² ≥ 0 for all x, equals 0 only at x = 0. Increasing on (-∞, +∞).",
            "(-∞, +∞)", "(0, +∞)", "(-∞, 0)", "(1, +∞)", -0.5));

        questions.Add(Q(T("16.1.2"), "f(x) = x² - 6x + 5 has minimum at:",
            D.Easy, "f'(x) = 2x - 6 = 0 → x = 3. f(3) = 9-18+5 = -4.",
            "x = 3, min = -4", "x = 3, min = -2", "x = 6, min = 5", "x = 0, min = 5", -0.3));

        questions.Add(Q(T("16.2.1"), "Box (no lid), square base x, volume 4. S = x² + 16/x. Min S = ?",
            D.Hard, "S' = 2x - 16/x² = 0 → x = 2. S(2) = 4 + 8 = 12.",
            "12", "8", "16", "10", 1.0));

        questions.Add(Q(T("16.1.1"), "If f'(x) > 0 on interval I, then f is:",
            D.Easy, "Positive derivative → increasing.",
            "Increasing on I", "Decreasing on I", "Constant on I", "Has a maximum on I", -0.8));

        questions.Add(Q(T("16.1.2"), "f(x) = -x² + 4x has a local maximum at:",
            D.Easy, "f'(x) = -2x + 4 = 0 → x = 2. f(2) = 4.",
            "x = 2, value = 4", "x = 4, value = 0", "x = 0, value = 0", "x = -2, value = -12", -0.3));

        questions.Add(Q(T("16.3.1"), "f(x) = x⁴ has an inflection point at:",
            D.Medium, "f''(x) = 12x² ≥ 0 everywhere, concavity doesn't change. No inflection point.",
            "No inflection point", "x = 0", "x = 1", "x = -1", 0.4));

        questions.Add(Q(T("16.2.2"), "The maximum of f(x) = sin x on [0, 2π] is at:",
            D.Easy, "sin x max = 1 at x = π/2.",
            "x = π/2", "x = π", "x = 0", "x = 3π/2", -0.8));

        questions.Add(Q(T("16.1.1"), "f(x) = ln x is concave on (0, +∞) because:",
            D.Medium, "f''(x) = -1/x² < 0 for all x > 0.",
            "f''(x) < 0 for all x > 0", "f'(x) > 0", "f(x) > 0", "f is increasing", 0.3));

        // ── Ch17 Permutations & Combinations (+9) ───────────────────

        questions.Add(Q(T("17.1.1"), "A lock has 4 digits, each 0-9. Total combinations:",
            D.Easy, "10⁴ = 10000.",
            "10000", "40", "10", "1000", -0.8));

        questions.Add(Q(T("17.1.2"), "P(n, n) = ?",
            D.Easy, "P(n,n) = n!",
            "n!", "nⁿ", "2ⁿ", "1", -0.8));

        questions.Add(Q(T("17.1.3"), "C(10, 3) = ?",
            D.Easy, "(10×9×8)/(3×2×1) = 120.",
            "120", "720", "30", "240", -0.3));

        questions.Add(Q(T("17.1.3"), "C(n, n) = ?",
            D.Easy, "One way to choose all n items: C(n,n) = 1.",
            "1", "n", "n!", "0", -1.2));

        questions.Add(Q(T("17.1.3"), "C(n, r) = C(n, ?) always holds:",
            D.Easy, "Symmetry: C(n, r) = C(n, n-r).",
            "n - r", "r + 1", "n + r", "r - 1", -0.5));

        questions.Add(Q(T("17.2.1"), "The general term of (a + b)ⁿ is:",
            D.Medium, "T_{r+1} = C(n,r)·aⁿ⁻ʳ·bʳ.",
            "C(n,r)·aⁿ⁻ʳ·bʳ", "C(n,r)·aʳ·bⁿ⁻ʳ", "n!·aⁿ⁻ʳ·bʳ", "C(n,r)·(ab)ʳ", 0.2));

        questions.Add(Q(T("17.2.2"), "C(n,0) + C(n,1) + ... + C(n,n) = ?",
            D.Easy, "By binomial theorem with a=b=1: 2ⁿ.",
            "2ⁿ", "n!", "nⁿ", "n²", -0.5));

        questions.Add(Q(T("17.1.2"), "How many ways can 5 people sit in a row?",
            D.Easy, "5! = 120.",
            "120", "25", "5", "720", -0.5));

        questions.Add(Q(T("17.1.1"), "Even 3-digit numbers from {1,2,3,4,5} without repetition:",
            D.Hard, "Last: 2 or 4 (2 choices). First: 3 remaining non-zero. Middle: 3 remaining. 2×3×3 = 18. Wait — first 4 choices (any of 5 minus last), middle 3. 2×4×3 = 24.",
            "24", "20", "30", "60", 1.0));

        // ── Ch18 Random Events & Probability (+12) ──────────────────

        questions.Add(Q(T("18.1.1"), "The probability of a certain event is:",
            D.Easy, "P(certain event) = 1.",
            "1", "0", "1/2", "∞", -1.5));

        questions.Add(Q(T("18.1.1"), "For any event A, 0 ≤ P(A) ≤ ?",
            D.Easy, "Probability is between 0 and 1.",
            "1", "∞", "100", "A", -1.5));

        questions.Add(Q(T("18.1.2"), "A coin is flipped 3 times. P(exactly 2 heads) = ?",
            D.Medium, "C(3,2)·(1/2)³ = 3/8.",
            "3/8", "1/4", "1/2", "1/8", 0.2));

        questions.Add(Q(T("18.1.2"), "A card from a 52-card deck. P(heart) = ?",
            D.Easy, "13/52 = 1/4.",
            "1/4", "1/13", "1/52", "1/2", -0.8));

        questions.Add(Q(T("18.2.1"), "For mutually exclusive A, B: P(A ∪ B) = ?",
            D.Easy, "P(A) + P(B).",
            "P(A) + P(B)", "P(A)·P(B)", "P(A) + P(B) - P(A∩B)", "P(A|B)·P(B)", -0.5));

        questions.Add(Q(T("18.2.1"), "P(A ∪ B) = P(A) + P(B) - P(A ∩ B) is called:",
            D.Easy, "Inclusion-exclusion principle.",
            "Inclusion-exclusion principle", "Bayes' theorem",
            "Law of total probability", "Multiplication rule", -0.5));

        questions.Add(Q(T("18.2.2"), "If A, B independent, P(Ā ∩ B̄) = ?",
            D.Medium, "(1-P(A))·(1-P(B)).",
            "(1-P(A))(1-P(B))", "1 - P(A) - P(B)", "P(A)·P(B)", "1 - P(A)P(B)", 0.2));

        questions.Add(Q(T("18.3.1"), "Two events are complementary if:",
            D.Easy, "A ∪ Ā = Ω and A ∩ Ā = ∅.",
            "A ∪ Ā = Ω and A ∩ Ā = ∅", "P(A) = P(Ā)", "A = Ā", "P(A) + P(Ā) = 0", -0.5));

        questions.Add(Q(T("18.1.2"), "From 3 boys, 2 girls, choose 2. P(both girls) = ?",
            D.Medium, "C(2,2)/C(5,2) = 1/10.",
            "1/10", "2/5", "1/5", "3/10", 0.2));

        questions.Add(Q(T("18.1.2"), "Three coins tossed. P(at least one head) = ?",
            D.Medium, "1 - P(no heads) = 1 - 1/8 = 7/8.",
            "7/8", "3/4", "1/2", "1/8", 0.2));

        questions.Add(Q(T("18.2.2"), "P(A)=0.6, P(B)=0.3, independent. P(A ∪ B) = ?",
            D.Medium, "0.6 + 0.3 - 0.18 = 0.72.",
            "0.72", "0.9", "0.18", "0.63", 0.3));

        questions.Add(Q(T("18.3.1"), "Total probability: if B₁,...,Bₙ partition Ω, P(A) = ?",
            D.Hard, "P(A) = Σ P(A|Bᵢ)P(Bᵢ).",
            "Σ P(A|Bᵢ)P(Bᵢ)", "Σ P(Bᵢ|A)P(A)", "P(A|B₁)+P(A|B₂)", "P(B₁)P(B₂)...P(Bₙ)", 1.0));

        // ── Ch19 Random Variables (+8) ──────────────────────────────

        questions.Add(Q(T("19.1.1"), "A random variable taking countable values is called:",
            D.Easy, "Discrete random variable.",
            "Discrete", "Continuous", "Normal", "Binomial", -1.0));

        questions.Add(Q(T("19.1.2"), "CDF F(x) satisfies F(-∞) = ? and F(+∞) = ?",
            D.Medium, "F(-∞) = 0 and F(+∞) = 1.",
            "F(-∞) = 0, F(+∞) = 1", "F(-∞) = -1, F(+∞) = 1",
            "F(-∞) = 0, F(+∞) = ∞", "F(-∞) = 1, F(+∞) = 0", 0.0));

        questions.Add(Q(T("19.2.1"), "If X is uniform on {1, 2, 3, 4}, then E(X) = ?",
            D.Easy, "(1+2+3+4)/4 = 2.5.",
            "2.5", "2", "3", "10", -0.5));

        questions.Add(Q(T("19.2.2"), "X takes 0, 1 with P(X=1) = p. Var(X) = ?",
            D.Medium, "Bernoulli: Var(X) = p(1-p).",
            "p(1-p)", "p²", "p", "1-p", 0.2));

        questions.Add(Q(T("19.3.1"), "If X ~ B(100, 0.05), P(X = 0) ≈ ?",
            D.Hard, "0.95¹⁰⁰ ≈ 0.00592.",
            "≈ 0.006", "≈ 0.05", "≈ 0.5", "≈ 0.95", 1.2));

        questions.Add(Q(T("19.3.2"), "If X ~ N(100, 25), P(95 < X < 105) ≈ ?",
            D.Medium, "σ = 5. P(μ-σ < X < μ+σ) ≈ 68.26%.",
            "≈ 68.26%", "≈ 95.44%", "≈ 50%", "≈ 34.13%", 0.3));

        questions.Add(Q(T("19.2.1"), "E(X + Y) for any X, Y equals:",
            D.Easy, "E(X + Y) = E(X) + E(Y) (linearity).",
            "E(X) + E(Y)", "E(X)·E(Y)", "E(XY)", "max(E(X), E(Y))", -0.5));

        questions.Add(Q(T("19.2.2"), "If X, Y independent, Var(X + Y) = ?",
            D.Medium, "Var(X) + Var(Y).",
            "Var(X) + Var(Y)", "Var(X)·Var(Y)", "[Var(X)+Var(Y)]²", "max(Var(X),Var(Y))", 0.2));

        // ── Ch20 Statistics (+8) ────────────────────────────────────

        questions.Add(Q(T("20.1.1"), "In simple random sampling, each individual has:",
            D.Easy, "Equal probability of being selected.",
            "Equal probability of selection", "Probability proportional to size",
            "Zero probability", "Probability of 1", -1.0));

        questions.Add(Q(T("20.1.2"), "The number of classes k for dataset of size n ≈:",
            D.Medium, "Sturges' rule: k ≈ 1 + 3.322 lg n.",
            "1 + 3.322 lg n", "n/2", "√n", "n - 1", 0.2));

        questions.Add(Q(T("20.2.1"), "If all values increase by 5, the mean:",
            D.Easy, "Adding constant c increases mean by c.",
            "Increases by 5", "Stays the same", "Doubles", "Increases by 25", -0.8));

        questions.Add(Q(T("20.2.1"), "If all values are multiplied by 2, the variance:",
            D.Medium, "Var(2X) = 4·Var(X).",
            "Is multiplied by 4", "Is multiplied by 2", "Stays the same", "Is multiplied by √2", 0.2));

        questions.Add(Q(T("20.2.2"), "Standard deviation is:",
            D.Easy, "The square root of variance.",
            "The square root of variance", "The mean of the data",
            "The range of the data", "The median of the data", -0.8));

        questions.Add(Q(T("20.3.1"), "In regression ŷ = bx + a, the line always passes through:",
            D.Medium, "Through the point (x̄, ȳ).",
            "(x̄, ȳ)", "(0, a)", "(b, a)", "(0, 0)", 0.0));

        questions.Add(Q(T("20.3.2"), "If r = 0, then X and Y have:",
            D.Easy, "No linear correlation.",
            "No linear correlation", "Perfect correlation",
            "Strong negative correlation", "No relationship at all", -0.5));

        questions.Add(Q(T("20.1.1"), "Stratified sampling divides the population into:",
            D.Easy, "Homogeneous strata, then samples from each.",
            "Homogeneous strata", "Random groups", "Equal-sized groups", "Overlapping groups", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // Save all questions
        // ═══════════════════════════════════════════════════════════════

        await _context.Questions.AddRangeAsync(questions);
        await _context.SaveChangesAsync();
    }

    // ─── Compact helpers ─────────────────────────────────────────────

    /// <summary>Alias for QuestionDifficulty to reduce verbosity.</summary>
    private static class D
    {
        public const QuestionDifficulty Easy = QuestionDifficulty.Easy;
        public const QuestionDifficulty Medium = QuestionDifficulty.Medium;
        public const QuestionDifficulty Hard = QuestionDifficulty.Hard;
    }

    /// <summary>
    /// Creates a Question with 4 answer options (1 correct, 3 distractors).
    /// The correct option is always first in the source but EF will assign IDs; the frontend should shuffle.
    /// </summary>
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
