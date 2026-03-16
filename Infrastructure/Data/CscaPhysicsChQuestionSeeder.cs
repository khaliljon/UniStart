using Microsoft.EntityFrameworkCore;
using UniStart.Domain.Entities;

namespace UniStart.Infrastructure.Data;

/// <summary>
/// Seeds ~120 multiple-choice questions for CSCA Physics CN (10 topics, Chinese language).
/// Each question has exactly 4 answer options, difficulty level, explanation, and IRT parameters.
/// </summary>
public class CscaPhysicsChQuestionSeeder
{
    private readonly UniStartDbContext _context;

    public CscaPhysicsChQuestionSeeder(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var physCnSection = await _context.ExamSections
            .FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name == "Physics (CN)");
        if (physCnSection == null) return;

        var existing = await _context.Questions
            .AnyAsync(q => q.Topic.SectionId == physCnSection.Id);
        if (existing) return;

        var topics = await _context.Topics
            .Where(t => t.SectionId == physCnSection.Id)
            .ToDictionaryAsync(t => t.Name.Split(' ')[0], t => t.Id);

        int T(string prefix) => topics.TryGetValue(prefix, out var id) ? id : topics.Values.First();

        var questions = new List<Question>();

        // ═══════════════════════════════════════════════════════════════
        // CN-P1 力学 (Mechanics)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-P1"), "关于力的概念，下列说法正确的是：",
            D.Easy, "力是物体对物体的作用，必须有施力物体和受力物体。",
            "力是物体对物体的作用", "力可以脱离物体而独立存在",
            "只有接触的物体才有力的作用", "力只能改变物体的形状", -1.0));

        questions.Add(Q(T("CN-P1"), "弹簧的劲度系数为 500 N/m，拉力为 100 N 时伸长量为：",
            D.Easy, "F = kx → x = F/k = 100/500 = 0.2 m。",
            "0.2 m", "2 m", "0.5 m", "5 m", -0.8));

        questions.Add(Q(T("CN-P1"), "一个物体受 3 N 和 4 N 两个互相垂直的力作用，合力大小为：",
            D.Easy, "F = √(3² + 4²) = √25 = 5 N。",
            "5 N", "7 N", "1 N", "12 N", -0.8));

        questions.Add(Q(T("CN-P1"), "关于摩擦力，下列说法正确的是：",
            D.Medium, "滑动摩擦力 f = μN，大小与接触面积无关。",
            "滑动摩擦力的大小与接触面积无关", "静摩擦力的方向一定与运动方向相反",
            "摩擦力一定阻碍物体运动", "接触面越光滑摩擦力一定越小", 0.3));

        questions.Add(Q(T("CN-P1"), "物体质量 m = 10 kg，重力加速度 g = 10 m/s²，则重力 G =",
            D.Easy, "G = mg = 10 × 10 = 100 N。",
            "100 N", "10 N", "1000 N", "1 N", -1.2));

        questions.Add(Q(T("CN-P1"), "两个力 F₁ 和 F₂ 的合力最大值为 F₁ + F₂，最小值为：",
            D.Easy, "合力最小值为 |F₁ - F₂|。",
            "|F₁ - F₂|", "0", "F₁ × F₂", "√(F₁² + F₂²)", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CN-P2 运动学 (Kinematics)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-P2"), "匀速直线运动中，速度 v = 5 m/s，4 秒内位移为：",
            D.Easy, "s = vt = 5 × 4 = 20 m。",
            "20 m", "9 m", "1.25 m", "25 m", -1.2));

        questions.Add(Q(T("CN-P2"), "物体做匀加速直线运动，初速度 v₀ = 0，加速度 a = 2 m/s²，3 秒后速度为：",
            D.Easy, "v = v₀ + at = 0 + 2 × 3 = 6 m/s。",
            "6 m/s", "3 m/s", "9 m/s", "2 m/s", -0.8));

        questions.Add(Q(T("CN-P2"), "自由落体运动中，物体下落 2 秒的位移是（g = 10 m/s²）：",
            D.Easy, "h = ½gt² = ½ × 10 × 4 = 20 m。",
            "20 m", "10 m", "40 m", "5 m", -0.8));

        questions.Add(Q(T("CN-P2"), "匀变速直线运动的位移公式 s =",
            D.Easy, "s = v₀t + ½at²。",
            "v₀t + ½at²", "vt", "v₀t - ½at²", "at²", -0.5));

        questions.Add(Q(T("CN-P2"), "关于加速度，下列说法正确的是：",
            D.Medium, "加速度表示速度变化的快慢，即单位时间内速度的变化量。",
            "加速度表示速度变化的快慢", "加速度越大速度一定越大",
            "加速度为零物体一定静止", "加速度方向一定与速度方向相同", 0.2));

        questions.Add(Q(T("CN-P2"), "v-t 图像中，图线斜率表示：",
            D.Easy, "v-t 图的斜率 = Δv/Δt = 加速度。",
            "加速度", "位移", "速度", "路程", -0.5));

        questions.Add(Q(T("CN-P2"), "物体从高处自由落下，落地速度 v = 30 m/s（g = 10 m/s²），则下落高度 h =",
            D.Medium, "v² = 2gh → h = v²/(2g) = 900/20 = 45 m。",
            "45 m", "90 m", "30 m", "15 m", 0.2));

        // ═══════════════════════════════════════════════════════════════
        // CN-P3 牛顿定律 (Newton's Laws)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-P3"), "牛顿第一定律内容是：",
            D.Easy, "一切物体总保持匀速直线运动或静止状态，除非外力迫使它改变这种状态。",
            "物体在不受力或合力为零时保持匀速直线运动或静止", "力是改变物体形状的原因",
            "作用力等于反作用力", "加速度与力成正比", -0.8));

        questions.Add(Q(T("CN-P3"), "质量 m = 5 kg 的物体受合力 F = 10 N，加速度 a =",
            D.Easy, "a = F/m = 10/5 = 2 m/s²。",
            "2 m/s²", "50 m/s²", "0.5 m/s²", "15 m/s²", -1.0));

        questions.Add(Q(T("CN-P3"), "关于牛顿第三定律，下列说法正确的是：",
            D.Medium, "作用力和反作用力大小相等、方向相反、作用在不同物体上、同时产生同时消失。",
            "作用力与反作用力大小相等方向相反，作用在不同物体上",
            "作用力与反作用力可以抵消",
            "作用力先于反作用力产生",
            "大小不一定相等", 0.2));

        questions.Add(Q(T("CN-P3"), "电梯加速上升时，人对秤的压力与重力相比：",
            D.Medium, "加速上升，视重 > 重力（超重现象）。N = m(g + a) > mg。",
            "大于重力（超重）", "等于重力", "小于重力（失重）", "为零", 0.3));

        questions.Add(Q(T("CN-P3"), "物体在粗糙水平面上受 F = 20 N 水平拉力做匀速运动，则摩擦力为：",
            D.Easy, "匀速运动合力为零，摩擦力 = 拉力 = 20 N（方向相反）。",
            "20 N", "0 N", "10 N", "40 N", -0.5));

        questions.Add(Q(T("CN-P3"), "一物体质量 2 kg，在光滑斜面上由静止下滑，斜面倾角 30°，加速度为（g = 10 m/s²）：",
            D.Medium, "a = g sin 30° = 10 × 0.5 = 5 m/s²。",
            "5 m/s²", "10 m/s²", "2.5 m/s²", "8.66 m/s²", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CN-P4 功和能 (Work & Energy)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-P4"), "力 F = 10 N 沿水平方向推物体移动 s = 5 m，做的功 W =",
            D.Easy, "W = Fs cos θ = 10 × 5 × cos 0° = 50 J。",
            "50 J", "15 J", "2 J", "100 J", -1.0));

        questions.Add(Q(T("CN-P4"), "动能的表达式为：",
            D.Easy, "Eₖ = ½mv²。",
            "½mv²", "mgh", "½kx²", "Fs", -0.8));

        questions.Add(Q(T("CN-P4"), "质量 m = 2 kg 的物体在高 h = 5 m 处的重力势能为（g = 10 m/s²）：",
            D.Easy, "Ep = mgh = 2 × 10 × 5 = 100 J。",
            "100 J", "50 J", "10 J", "200 J", -0.8));

        questions.Add(Q(T("CN-P4"), "功率的定义是：",
            D.Easy, "功率 P = W/t，表示单位时间内做的功。",
            "单位时间内做的功", "力和位移的乘积",
            "能量的大小", "速度的变化率", -0.5));

        questions.Add(Q(T("CN-P4"), "机械能守恒的条件是：",
            D.Medium, "只有重力或弹力做功时（无摩擦等非保守力做功），机械能守恒。",
            "只有重力或弹力做功", "没有任何力做功",
            "动能不变", "速度不变", 0.3));

        questions.Add(Q(T("CN-P4"), "物体从 10 m 高处自由落下，落地瞬间的动能等于（忽略空气阻力）：",
            D.Medium, "机械能守恒：Eₖ = mgh。从 10 m 落下：Eₖ = 初始势能 = mgh。",
            "mgh = 10mg", "½mgh", "2mgh", "mg/10", 0.2));

        // ═══════════════════════════════════════════════════════════════
        // CN-P5 电场 (Electric Fields)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-P5"), "库仑定律公式为：",
            D.Easy, "F = kq₁q₂/r²，k 为库仑常数。",
            "F = kq₁q₂/r²", "F = kq₁q₂/r", "F = q₁q₂/r²", "F = kq₁q₂r²", -0.8));

        questions.Add(Q(T("CN-P5"), "电场强度的定义为：",
            D.Easy, "E = F/q，即单位正电荷受到的电场力。",
            "E = F/q", "E = U/q", "E = W/q", "E = kq/r", -0.5));

        questions.Add(Q(T("CN-P5"), "电场线从正电荷出发，终止于：",
            D.Easy, "电场线从正电荷出发到负电荷终止（或延伸到无穷远）。",
            "负电荷", "正电荷", "中性物体", "磁铁的北极", -0.8));

        questions.Add(Q(T("CN-P5"), "匀强电场中，电场强度 E 与电势差 U 的关系为：",
            D.Medium, "E = U/d，其中 d 为沿电场方向的两点间距离。",
            "E = U/d", "E = Ud", "E = U/q", "E = qU", 0.2));

        questions.Add(Q(T("CN-P5"), "一个正电荷从 A 点移到 B 点，电场力做正功，则：",
            D.Medium, "电场力做正功说明电势能减小，A 点电势高于 B 点。",
            "A 点电势高于 B 点", "B 点电势高于 A 点",
            "A、B 两点电势相等", "无法判断", 0.3));

        questions.Add(Q(T("CN-P5"), "平行板电容器的电容公式 C =",
            D.Medium, "C = εS/4πkd，或简写 C = εₒεᵣS/d。",
            "εS/(4πkd)", "Q/U（定义式）两者均可",
            "kQ/r²", "Ed", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CN-P6 电路 (Circuits)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-P6"), "欧姆定律的表达式为：",
            D.Easy, "I = U/R（电流 = 电压/电阻）。",
            "I = U/R", "U = IR²", "R = IU", "P = UI", -1.0));

        questions.Add(Q(T("CN-P6"), "两个电阻 R₁ = 4 Ω 和 R₂ = 6 Ω 串联，总电阻 R =",
            D.Easy, "串联：R = R₁ + R₂ = 4 + 6 = 10 Ω。",
            "10 Ω", "2.4 Ω", "24 Ω", "5 Ω", -1.0));

        questions.Add(Q(T("CN-P6"), "两个电阻 R₁ = 4 Ω 和 R₂ = 12 Ω 并联，总电阻 R =",
            D.Easy, "并联：1/R = 1/4 + 1/12 = 3/12 + 1/12 = 4/12 → R = 3 Ω。",
            "3 Ω", "16 Ω", "8 Ω", "48 Ω", -0.5));

        questions.Add(Q(T("CN-P6"), "电功率的计算公式为：",
            D.Easy, "P = UI = I²R = U²/R。",
            "P = UI", "P = mgh/t", "P = Fv", "P = ½mv²", -0.8));

        questions.Add(Q(T("CN-P6"), "一个灯泡标有 '220V 60W'，正常工作时电流为：",
            D.Medium, "I = P/U = 60/220 ≈ 0.27 A ≈ 3/11 A。",
            "3/11 A（约 0.27 A）", "60 A", "3.67 A", "220 A", 0.2));

        questions.Add(Q(T("CN-P6"), "电池内阻为 r，外电路电阻为 R，电动势为 ε，则路端电压 U =",
            D.Medium, "U = ε - Ir = εR/(R + r)。",
            "ε - Ir", "ε + Ir", "IR", "ε/R", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CN-P7 磁场 (Magnetic Fields)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-P7"), "安培力公式为：",
            D.Easy, "F = BIL（当电流方向与磁场垂直时）。",
            "F = BIL", "F = qvB", "F = kq₁q₂/r²", "F = mg", -0.5));

        questions.Add(Q(T("CN-P7"), "洛伦兹力的方向可用什么方法判断？",
            D.Easy, "左手定则：四指指向正电荷运动方向（或反向负电荷运动方向），弯向磁场方向，拇指为力的方向。",
            "左手定则", "右手定则", "安培定则", "楞次定律", -0.5));

        questions.Add(Q(T("CN-P7"), "带电粒子在匀强磁场中做匀速圆周运动的半径 r =",
            D.Medium, "qvB = mv²/r → r = mv/(qB)。",
            "mv/(qB)", "qvB/m", "mv²/(qB)", "qB/(mv)", 0.3));

        questions.Add(Q(T("CN-P7"), "磁感应强度的国际单位是：",
            D.Easy, "磁感应强度 B 的单位是特斯拉（T），1 T = 1 kg/(A·s²)。",
            "特斯拉（T）", "高斯（G）", "韦伯（Wb）", "安培（A）", -0.8));

        questions.Add(Q(T("CN-P7"), "通电直导线周围磁场方向的判断用：",
            D.Easy, "安培定则（右手螺旋定则）：右手拇指指向电流方向，四指弯曲方向即磁场方向。",
            "安培定则（右手螺旋定则）", "左手定则", "楞次定律", "法拉第定律", -0.5));

        questions.Add(Q(T("CN-P7"), "洛伦兹力对带电粒子做的功为：",
            D.Easy, "洛伦兹力始终垂直于速度方向，所以不做功。",
            "零", "正功", "负功", "不确定", -0.3));

        // ═══════════════════════════════════════════════════════════════
        // CN-P8 电磁感应 (Electromagnetic Induction)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-P8"), "法拉第电磁感应定律：感应电动势 ε =",
            D.Easy, "ε = -NΔΦ/Δt（感应电动势等于磁通量变化率的N倍）。",
            "NΔΦ/Δt", "BIL", "IR", "qvB", -0.5));

        questions.Add(Q(T("CN-P8"), "楞次定律的内容是：",
            D.Medium, "感应电流的方向总是使得它所产生的磁场阻碍引起该感应电流的磁通量的变化。",
            "感应电流的磁场阻碍引起感应电流的磁通量变化",
            "感应电流的方向与磁场方向相同",
            "感应电流总是使磁通量增大",
            "感应电动势与磁通量成正比", 0.3));

        questions.Add(Q(T("CN-P8"), "导体棒在匀强磁场中做切割磁力线运动，ε = BLv 中 v 是：",
            D.Easy, "v 是导体棒垂直于磁场和导体棒方向运动的速度分量。",
            "导体棒垂直于磁场方向的运动速度", "导体棒沿磁场方向的速度",
            "电流的速度", "磁场变化的速度", -0.3));

        questions.Add(Q(T("CN-P8"), "磁通量 Φ 的单位是：",
            D.Easy, "磁通量的单位是韦伯（Wb），1 Wb = 1 T·m²。",
            "韦伯（Wb）", "特斯拉（T）", "伏特（V）", "安培（A）", -0.8));

        questions.Add(Q(T("CN-P8"), "自感现象中，线圈电流变化时产生的电动势称为：",
            D.Medium, "线圈自身电流变化引起的感应电动势称为自感电动势。",
            "自感电动势", "互感电动势", "反电动势", "动生电动势", 0.2));

        questions.Add(Q(T("CN-P8"), "变压器的电压与匝数的关系为：",
            D.Easy, "U₁/U₂ = N₁/N₂（理想变压器）。",
            "U₁/U₂ = N₁/N₂", "U₁/U₂ = N₂/N₁", "U₁N₁ = U₂N₂", "U₁ + U₂ = N₁ + N₂", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CN-P9 光学 (Optics)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-P9"), "光的反射定律是：",
            D.Easy, "入射角等于反射角，入射光线、反射光线和法线在同一平面内。",
            "入射角等于反射角，且在同一平面内", "入射角大于反射角",
            "反射角等于折射角", "光总沿直线传播", -1.0));

        questions.Add(Q(T("CN-P9"), "光从空气射入水中时，折射角与入射角的关系是：",
            D.Easy, "光从光疏介质进入光密介质，折射角小于入射角。",
            "折射角小于入射角", "折射角大于入射角",
            "折射角等于入射角", "不确定", -0.5));

        questions.Add(Q(T("CN-P9"), "全反射发生的条件是：",
            D.Medium, "光从光密介质射向光疏介质，且入射角大于临界角。",
            "光从光密介质到光疏介质，入射角大于临界角",
            "光从光疏介质到光密介质",
            "入射角小于临界角",
            "光的频率足够高", 0.3));

        questions.Add(Q(T("CN-P9"), "凸透镜对光的作用是：",
            D.Easy, "凸透镜对光线有会聚作用。",
            "会聚", "发散", "既不会聚也不发散", "只反射", -1.0));

        questions.Add(Q(T("CN-P9"), "凸透镜成像中，物距大于 2 倍焦距时，成像特点是：",
            D.Medium, "u > 2f 时，成倒立、缩小的实像。",
            "倒立缩小的实像", "正立放大的虚像", "倒立放大的实像", "正立缩小的虚像", 0.2));

        questions.Add(Q(T("CN-P9"), "光的干涉现象说明光具有：",
            D.Easy, "干涉是波特有的现象，说明光具有波动性。",
            "波动性", "粒子性", "直线传播性", "热效应", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CN-P10 热学 (Thermodynamics)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-P10"), "热力学第一定律的表达式为：",
            D.Easy, "ΔU = Q + W（内能变化 = 吸收热量 + 外界做的功）。",
            "ΔU = Q + W", "ΔU = Q - W", "Q = W", "ΔU = 0", -0.5));

        questions.Add(Q(T("CN-P10"), "关于温度，下列说法正确的是：",
            D.Easy, "温度是大量分子热运动平均动能的标志。",
            "温度是分子平均动能的标志", "温度越高分子数越多",
            "温度是分子势能的标志", "0°C 时分子停止运动", -0.5));

        questions.Add(Q(T("CN-P10"), "理想气体等温变化时满足：",
            D.Easy, "等温变化：p₁V₁ = p₂V₂（玻意耳定律）。",
            "p₁V₁ = p₂V₂", "V₁/T₁ = V₂/T₂", "p₁/T₁ = p₂/T₂", "pV/T = 常数", -0.5));

        questions.Add(Q(T("CN-P10"), "热力学第二定律表明：",
            D.Medium, "热量不能自发地从低温物体传到高温物体（克劳修斯说法）。",
            "热量不能自发地从低温物体传到高温物体",
            "能量可以被创造",
            "热机效率可以达到 100%",
            "所有过程都是可逆的", 0.2));

        questions.Add(Q(T("CN-P10"), "布朗运动反映了什么？",
            D.Medium, "布朗运动是悬浮颗粒受到液体分子不均匀撞击导致的，反映了液体分子的无规则运动。",
            "液体分子的无规则运动", "悬浮颗粒自身的运动",
            "液体的流动", "温度不均匀", 0.3));

        questions.Add(Q(T("CN-P10"), "在绝热过程中，外界对气体做功 500 J，则气体内能变化为：",
            D.Easy, "绝热过程 Q = 0，ΔU = W = 500 J（内能增加）。",
            "增加 500 J", "减少 500 J", "不变", "增加 250 J", -0.3));

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
