using Microsoft.EntityFrameworkCore;
using UniStart.Domain.Entities;

namespace UniStart.Infrastructure.Data;

/// <summary>
/// Seeds ~120 multiple-choice questions for CSCA Mathematics CN (10 topics, Chinese language).
/// Each question has exactly 4 answer options, difficulty level, explanation, and IRT parameters.
/// </summary>
public class CscaMathChQuestionSeeder
{
    private readonly UniStartDbContext _context;

    public CscaMathChQuestionSeeder(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var mathCnSection = await _context.ExamSections
            .FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name == "Mathematics (CN)");
        if (mathCnSection == null) return;

        var existing = await _context.Questions
            .AnyAsync(q => q.Topic.SectionId == mathCnSection.Id);
        if (existing) return;

        var topics = await _context.Topics
            .Where(t => t.SectionId == mathCnSection.Id)
            .ToDictionaryAsync(t => t.Name.Split(' ')[0], t => t.Id);

        int T(string prefix) => topics.TryGetValue(prefix, out var id) ? id : topics.Values.First();

        var questions = new List<Question>();

        // ═══════════════════════════════════════════════════════════════
        // CN-M1 集合 (Sets)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-M1"), "设 A = {x | -3 < x ≤ 5}, B = {x | x < -1 或 x > 3}，则 A ∩ B =",
            D.Easy, "A ∩ B 取两个集合的公共部分：(-3, -1) ∪ (3, 5]。",
            "(-3, -1) ∪ (3, 5]", "(-3, 5]", "(-1, 3)", "(-∞, -1) ∪ (3, +∞)", -0.8));

        questions.Add(Q(T("CN-M1"), "集合 {a, b, c} 的子集个数为：",
            D.Easy, "含 n 个元素的集合有 2ⁿ 个子集。2³ = 8。",
            "8", "6", "7", "3", -1.0));

        questions.Add(Q(T("CN-M1"), "设全集 U = {1,2,3,4,5}，A = {1,3,5}，则 ∁ᵤA =",
            D.Easy, "补集包含 U 中不在 A 里的元素：{2, 4}。",
            "{2, 4}", "{1, 3, 5}", "{1, 2, 3, 4, 5}", "{}", -1.2));

        questions.Add(Q(T("CN-M1"), "若 A ∪ B = A，则下列结论正确的是：",
            D.Medium, "A ∪ B = A 意味着 B 的所有元素都在 A 中，即 B ⊆ A。",
            "B ⊆ A", "A ⊆ B", "A = ∅", "B = ∅", 0.3));

        questions.Add(Q(T("CN-M1"), "已知 A = {x | x² - 4 = 0}，则 A =",
            D.Easy, "x² - 4 = 0 → x = ±2。",
            "{-2, 2}", "{2}", "{4}", "{-4, 4}", -0.8));

        questions.Add(Q(T("CN-M1"), "设 A = {1, 2, 3}，B = {2, 3, 4}，则 A ∩ B =",
            D.Easy, "A ∩ B 取公共元素：{2, 3}。",
            "{2, 3}", "{1, 2, 3, 4}", "{1, 4}", "{4}", -1.2));

        // ═══════════════════════════════════════════════════════════════
        // CN-M2 函数 (Functions)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-M2"), "函数 y = √(x - 2) 的定义域是：",
            D.Easy, "根号下 x - 2 ≥ 0 → x ≥ 2。定义域：[2, +∞)。",
            "[2, +∞)", "(2, +∞)", "(-∞, 2]", "R", -0.8));

        questions.Add(Q(T("CN-M2"), "函数 f(x) = x² 的图像关于什么对称？",
            D.Easy, "f(-x) = (-x)² = x² = f(x)，所以是偶函数，图像关于 y 轴对称。",
            "y 轴", "x 轴", "原点", "直线 y = x", -0.5));

        questions.Add(Q(T("CN-M2"), "若 f(x) = 2x - 1，则 f(3) =",
            D.Easy, "f(3) = 2(3) - 1 = 5。",
            "5", "7", "3", "6", -1.2));

        questions.Add(Q(T("CN-M2"), "函数 f(x) = -x³ 是：",
            D.Easy, "f(-x) = -(-x)³ = x³ = -f(x)，所以是奇函数。",
            "奇函数", "偶函数", "非奇非偶", "既奇又偶", -0.5));

        questions.Add(Q(T("CN-M2"), "函数 y = 1/x 的定义域是：",
            D.Easy, "x ≠ 0，定义域为 (-∞, 0) ∪ (0, +∞)。",
            "(-∞, 0) ∪ (0, +∞)", "R", "[0, +∞)", "(-∞, 0)", -0.8));

        questions.Add(Q(T("CN-M2"), "若函数 f(x) 在 (0, +∞) 上单调递增，则称 f(x) 在该区间上是：",
            D.Easy, "单调递增函数即增函数。",
            "增函数", "减函数", "常函数", "周期函数", -1.0));

        // ═══════════════════════════════════════════════════════════════
        // CN-M3 指数与对数 (Exponents & Logarithms)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-M3"), "化简 2³ × 2² =",
            D.Easy, "同底数相乘，指数相加：2³⁺² = 2⁵ = 32。",
            "32", "64", "16", "10", -1.2));

        questions.Add(Q(T("CN-M3"), "log₂ 8 =",
            D.Easy, "2³ = 8，所以 log₂ 8 = 3。",
            "3", "2", "4", "8", -1.0));

        questions.Add(Q(T("CN-M3"), "若 log₃ x = 2，则 x =",
            D.Easy, "3² = 9，所以 x = 9。",
            "9", "6", "3", "8", -0.8));

        questions.Add(Q(T("CN-M3"), "lg 100 =",
            D.Easy, "lg 表示以 10 为底的对数。10² = 100，所以 lg 100 = 2。",
            "2", "10", "100", "1", -1.0));

        questions.Add(Q(T("CN-M3"), "化简 (a²)³ =",
            D.Easy, "幂的幂，指数相乘：a²ˣ³ = a⁶。",
            "a⁶", "a⁵", "a⁸", "a⁹", -1.0));

        questions.Add(Q(T("CN-M3"), "函数 y = 2ˣ 的值域是：",
            D.Easy, "指数函数 y = 2ˣ 的值域为 (0, +∞)。",
            "(0, +∞)", "R", "[0, +∞)", "(-∞, 0)", -0.5));

        questions.Add(Q(T("CN-M3"), "设 a = log₂ 3, b = log₂ 5，则 log₂ 15 =",
            D.Medium, "log₂ 15 = log₂(3 × 5) = log₂ 3 + log₂ 5 = a + b。",
            "a + b", "ab", "a - b", "a/b", 0.3));

        questions.Add(Q(T("CN-M3"), "若 0.5ˣ > 1，则 x 的范围是：",
            D.Medium, "底数 0.5 < 1，指数函数递减。0.5ˣ > 1 = 0.5⁰ → x < 0。",
            "x < 0", "x > 0", "x > 1", "x < -1", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CN-M4 三角函数 (Trigonometry)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-M4"), "sin 30° =",
            D.Easy, "sin 30° = 1/2。",
            "1/2", "√2/2", "√3/2", "1", -1.5));

        questions.Add(Q(T("CN-M4"), "cos 60° =",
            D.Easy, "cos 60° = 1/2。",
            "1/2", "√3/2", "0", "1", -1.2));

        questions.Add(Q(T("CN-M4"), "tan 45° =",
            D.Easy, "tan 45° = sin 45°/cos 45° = 1。",
            "1", "0", "√3", "1/2", -1.2));

        questions.Add(Q(T("CN-M4"), "将 120° 转化为弧度是：",
            D.Easy, "120° × π/180° = 2π/3。",
            "2π/3", "π/3", "3π/4", "π/2", -0.8));

        questions.Add(Q(T("CN-M4"), "sin²α + cos²α =",
            D.Easy, "基本三角恒等式：sin²α + cos²α = 1。",
            "1", "0", "2", "sin 2α", -1.5));

        questions.Add(Q(T("CN-M4"), "函数 y = sin x 的最小正周期是：",
            D.Easy, "正弦函数的周期 T = 2π。",
            "2π", "π", "π/2", "4π", -0.8));

        questions.Add(Q(T("CN-M4"), "已知 sin α = 3/5，α 为第一象限角，则 cos α =",
            D.Medium, "cos²α = 1 - sin²α = 1 - 9/25 = 16/25。α 在第一象限，cos α > 0，所以 cos α = 4/5。",
            "4/5", "3/5", "-4/5", "5/3", 0.2));

        questions.Add(Q(T("CN-M4"), "函数 y = 2sin(2x + π/6) 的振幅是：",
            D.Easy, "y = A sin(ωx + φ) 中 A = 2 即振幅。",
            "2", "1", "π/6", "4", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CN-M5 平面向量 (Plane Vectors)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-M5"), "若 a⃗ = (1, 2)，b⃗ = (3, 4)，则 a⃗ + b⃗ =",
            D.Easy, "向量加法：(1+3, 2+4) = (4, 6)。",
            "(4, 6)", "(3, 8)", "(2, 6)", "(4, 8)", -1.0));

        questions.Add(Q(T("CN-M5"), "向量 a⃗ = (3, 4) 的模 |a⃗| =",
            D.Easy, "|a⃗| = √(3² + 4²) = √(9 + 16) = √25 = 5。",
            "5", "7", "√7", "25", -0.8));

        questions.Add(Q(T("CN-M5"), "若 a⃗ = (1, 2)，则 2a⃗ =",
            D.Easy, "数乘向量：2(1, 2) = (2, 4)。",
            "(2, 4)", "(1, 4)", "(3, 4)", "(2, 2)", -1.2));

        questions.Add(Q(T("CN-M5"), "a⃗ = (1, 0)，b⃗ = (0, 1)，则 a⃗ · b⃗ =",
            D.Easy, "点积：1×0 + 0×1 = 0。两向量垂直。",
            "0", "1", "-1", "2", -1.0));

        questions.Add(Q(T("CN-M5"), "若 a⃗ · b⃗ = 0，则两向量的关系是：",
            D.Easy, "向量点积为 0 说明两向量互相垂直。",
            "互相垂直", "互相平行", "方向相同", "共线", -0.5));

        questions.Add(Q(T("CN-M5"), "向量 a⃗ = (2, -1)，b⃗ = (4, -2)，它们的关系是：",
            D.Medium, "b⃗ = 2a⃗，所以 a⃗ ∥ b⃗（平行/共线）。",
            "平行（共线）", "垂直", "既不平行也不垂直", "相等", 0.2));

        // ═══════════════════════════════════════════════════════════════
        // CN-M6 数列 (Sequences & Series)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-M6"), "等差数列 {aₙ} 中，a₁ = 2，公差 d = 3，则 a₅ =",
            D.Easy, "a₅ = a₁ + 4d = 2 + 12 = 14。",
            "14", "17", "11", "15", -0.8));

        questions.Add(Q(T("CN-M6"), "等差数列前 n 项和公式 Sₙ =",
            D.Easy, "Sₙ = n(a₁ + aₙ)/2 或 Sₙ = na₁ + n(n-1)d/2。",
            "n(a₁ + aₙ)/2", "n·a₁·aₙ", "a₁·rⁿ", "(a₁ + aₙ)·n", -0.5));

        questions.Add(Q(T("CN-M6"), "等比数列 {aₙ} 中，a₁ = 3，公比 q = 2，则 a₄ =",
            D.Easy, "a₄ = a₁ × q³ = 3 × 8 = 24。",
            "24", "12", "48", "18", -0.5));

        questions.Add(Q(T("CN-M6"), "等差数列中，若 a₃ = 5，a₇ = 13，则公差 d =",
            D.Medium, "a₇ = a₃ + 4d → 13 = 5 + 4d → d = 2。",
            "2", "4", "8", "3", 0.2));

        questions.Add(Q(T("CN-M6"), "等比数列 1, 2, 4, 8, ... 的公比 q =",
            D.Easy, "q = a₂/a₁ = 2/1 = 2。",
            "2", "4", "1", "8", -1.0));

        questions.Add(Q(T("CN-M6"), "等差数列 2, 5, 8, 11, ... 的第 10 项是：",
            D.Easy, "a₁ = 2，d = 3。a₁₀ = 2 + 9×3 = 29。",
            "29", "32", "30", "27", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CN-M7 不等式 (Inequalities)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-M7"), "不等式 2x - 6 > 0 的解集为：",
            D.Easy, "2x > 6 → x > 3。",
            "(3, +∞)", "[3, +∞)", "(-∞, 3)", "(-∞, 3]", -1.2));

        questions.Add(Q(T("CN-M7"), "不等式 x² - 5x + 6 < 0 的解集为：",
            D.Medium, "x² - 5x + 6 = (x-2)(x-3) = 0。根 x=2, x=3。开口向上，< 0 时：(2, 3)。",
            "(2, 3)", "(-∞, 2) ∪ (3, +∞)", "[2, 3]", "(-3, -2)", 0.2));

        questions.Add(Q(T("CN-M7"), "若 a > b > 0，则下列正确的是：",
            D.Easy, "a > b > 0 时 a² > b²（两边同正取平方保持不等号方向）。",
            "a² > b²", "1/a > 1/b", "-a > -b", "a² < b²", -0.5));

        questions.Add(Q(T("CN-M7"), "不等式 (x+1)(x-4) ≥ 0 的解集为：",
            D.Easy, "根 x = -1, x = 4。≥ 0 时：x ≤ -1 或 x ≥ 4。",
            "(-∞, -1] ∪ [4, +∞)", "[-1, 4]", "(-1, 4)", "(-∞, -4] ∪ [1, +∞)", -0.5));

        questions.Add(Q(T("CN-M7"), "若 a > b，c < 0，则：",
            D.Easy, "不等式两边乘以负数，不等号变向：ac < bc。",
            "ac < bc", "ac > bc", "ac = bc", "不能确定", -0.5));

        questions.Add(Q(T("CN-M7"), "不等式 |x - 1| < 3 的解集为：",
            D.Easy, "|x - 1| < 3 → -3 < x - 1 < 3 → -2 < x < 4。",
            "(-2, 4)", "(-4, 2)", "(-3, 3)", "(1, 4)", -0.3));

        // ═══════════════════════════════════════════════════════════════
        // CN-M8 立体几何 (Solid Geometry)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-M8"), "正方体的棱长为 a，则体积 V =",
            D.Easy, "正方体体积 V = a³。",
            "a³", "6a²", "3a", "a²", -1.2));

        questions.Add(Q(T("CN-M8"), "圆柱的体积公式为：",
            D.Easy, "V = πr²h。",
            "πr²h", "2πrh", "πr²", "4πr³/3", -0.8));

        questions.Add(Q(T("CN-M8"), "球的体积公式为：",
            D.Easy, "V = 4πr³/3。",
            "4πr³/3", "πr²h", "4πr²", "2πr³", -0.5));

        questions.Add(Q(T("CN-M8"), "正方体棱长为 2，则对角线长为：",
            D.Medium, "体对角线 = a√3 = 2√3。",
            "2√3", "2√2", "4", "6", 0.2));

        questions.Add(Q(T("CN-M8"), "若一个平面和一条直线没有公共点，则它们的关系是：",
            D.Easy, "直线和平面无公共点即平行。",
            "平行", "相交", "在平面内", "垂直", -0.8));

        questions.Add(Q(T("CN-M8"), "两个平面平行的判定定理需要：",
            D.Medium, "一个平面内的两条相交直线分别平行于另一个平面，则两平面平行。",
            "一个平面内两条相交直线分别平行于另一平面", "两平面内各有一条平行直线",
            "两平面的法向量互相垂直", "两平面无公共点即可", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CN-M9 解析几何 (Analytic Geometry)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-M9"), "圆 (x-1)² + (y+2)² = 9 的圆心和半径分别是：",
            D.Easy, "圆心 (1, -2)，半径 r = √9 = 3。",
            "圆心 (1, -2)，半径 3", "圆心 (-1, 2)，半径 3",
            "圆心 (1, -2)，半径 9", "圆心 (-1, 2)，半径 9", -0.8));

        questions.Add(Q(T("CN-M9"), "直线 y = 2x + 1 的斜率为：",
            D.Easy, "y = kx + b 中 k = 2。",
            "2", "1", "-2", "1/2", -1.2));

        questions.Add(Q(T("CN-M9"), "两点 A(1, 2) 和 B(4, 6) 的中点坐标为：",
            D.Easy, "中点 = ((1+4)/2, (2+6)/2) = (5/2, 4)。",
            "(5/2, 4)", "(3, 4)", "(5, 8)", "(2.5, 3)", -0.5));

        questions.Add(Q(T("CN-M9"), "椭圆 x²/9 + y²/4 = 1 的长半轴 a =",
            D.Easy, "a² = 9 → a = 3（取较大值对应的半轴）。",
            "3", "9", "2", "4", -0.5));

        questions.Add(Q(T("CN-M9"), "抛物线 y² = 4x 的焦点坐标为：",
            D.Medium, "y² = 4x → 2p = 4 → p = 2，焦点 (p/2, 0) = (1, 0)。",
            "(1, 0)", "(2, 0)", "(0, 1)", "(4, 0)", 0.3));

        questions.Add(Q(T("CN-M9"), "两条平行直线 y = 3x + 1 和 y = 3x - 2 之间的距离为：",
            D.Medium, "写成 3x - y + 1 = 0 和 3x - y - 2 = 0。d = |1-(-2)|/√(9+1) = 3/√10 = 3√10/10。",
            "3√10/10", "3", "√10", "3/10", 0.5));

        // ═══════════════════════════════════════════════════════════════
        // CN-M10 概率与统计 (Probability & Statistics)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-M10"), "抛一枚均匀硬币，正面朝上的概率是：",
            D.Easy, "等可能事件，P = 1/2。",
            "1/2", "1", "0", "1/4", -1.5));

        questions.Add(Q(T("CN-M10"), "掷一个均匀骰子，出现偶数的概率是：",
            D.Easy, "偶数 {2, 4, 6}，共 3 个，总 6 个面。P = 3/6 = 1/2。",
            "1/2", "1/3", "1/6", "2/3", -1.0));

        questions.Add(Q(T("CN-M10"), "从 {1,2,3,4,5} 中随机取 2 个数，取到的两数之和为偶数的概率是：",
            D.Medium, "C(5,2) = 10。和为偶数：两奇或两偶。奇数 {1,3,5} 取 2：C(3,2) = 3；偶数 {2,4} 取 2：C(2,2) = 1。P = 4/10 = 2/5。",
            "2/5", "3/5", "1/2", "3/10", 0.5));

        questions.Add(Q(T("CN-M10"), "数据 3, 5, 7, 5, 10 的平均数是：",
            D.Easy, "(3+5+7+5+10)/5 = 30/5 = 6。",
            "6", "5", "7", "30", -0.8));

        questions.Add(Q(T("CN-M10"), "数据 2, 3, 3, 5, 7 的中位数是：",
            D.Easy, "已排序，中间值为第 3 个：3。",
            "3", "5", "4", "2", -1.0));

        questions.Add(Q(T("CN-M10"), "数据 1, 2, 3, 4, 5 的方差为：",
            D.Medium, "均值 = 3。方差 = [(1-3)²+(2-3)²+(3-3)²+(4-3)²+(5-3)²]/5 = (4+1+0+1+4)/5 = 2。",
            "2", "√2", "10", "5", 0.3));

        questions.Add(Q(T("CN-M10"), "互斥事件 A 和 B 满足 P(A ∪ B) =",
            D.Easy, "互斥事件：P(A ∪ B) = P(A) + P(B)。",
            "P(A) + P(B)", "P(A) × P(B)", "P(A) - P(B)", "P(A)/P(B)", -0.5));

        questions.Add(Q(T("CN-M10"), "某次考试成绩服从正态分布 N(70, 100)，则标准差 σ =",
            D.Easy, "N(μ, σ²) 中 σ² = 100 → σ = 10。",
            "10", "100", "70", "√70", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // Save all questions
        // ═══════════════════════════════════════════════════════════════

        await _context.Questions.AddRangeAsync(questions);
        await _context.SaveChangesAsync();
    }

    // ─── Compact helpers ─────────────────────────────────────────────

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
