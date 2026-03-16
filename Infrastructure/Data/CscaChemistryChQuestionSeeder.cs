using Microsoft.EntityFrameworkCore;
using UniStart.Domain.Entities;

namespace UniStart.Infrastructure.Data;

/// <summary>
/// Seeds ~120 multiple-choice questions for CSCA Chemistry CN (10 topics, Chinese language).
/// Each question has exactly 4 answer options, difficulty level, explanation, and IRT parameters.
/// </summary>
public class CscaChemistryChQuestionSeeder
{
    private readonly UniStartDbContext _context;

    public CscaChemistryChQuestionSeeder(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var chemCnSection = await _context.ExamSections
            .FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name == "Chemistry (CN)");
        if (chemCnSection == null) return;

        var existing = await _context.Questions
            .AnyAsync(q => q.Topic.SectionId == chemCnSection.Id);
        if (existing) return;

        var topics = await _context.Topics
            .Where(t => t.SectionId == chemCnSection.Id)
            .ToDictionaryAsync(t => t.Name.Split(' ')[0], t => t.Id);

        int T(string prefix) => topics.TryGetValue(prefix, out var id) ? id : topics.Values.First();

        var questions = new List<Question>();

        // ═══════════════════════════════════════════════════════════════
        // CN-C1 原子结构 (Atomic Structure)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-C1"), "原子由哪些粒子组成？",
            D.Easy, "原子由原子核（质子和中子）和核外电子组成。",
            "质子、中子和电子", "质子和电子", "中子和电子", "只有质子", -1.2));

        questions.Add(Q(T("CN-C1"), "决定元素种类的是：",
            D.Easy, "质子数（原子序数）决定元素种类。",
            "质子数", "中子数", "电子数", "质量数", -1.0));

        questions.Add(Q(T("CN-C1"), "同位素是指：",
            D.Easy, "质子数相同而中子数不同的同种元素的不同原子。",
            "质子数相同中子数不同的原子", "中子数相同质子数不同的原子",
            "电子数相同的不同元素", "质量数相同的不同原子", -0.5));

        questions.Add(Q(T("CN-C1"), "碳-14 有 6 个质子，其中子数为：",
            D.Easy, "质量数 = 质子数 + 中子数 → 14 = 6 + n → n = 8。",
            "8", "6", "14", "12", -0.8));

        questions.Add(Q(T("CN-C1"), "Na 原子的核外电子排布为：",
            D.Easy, "Na 原子序数 11，电子排布 2, 8, 1。",
            "2, 8, 1", "2, 8, 3", "2, 7, 2", "8, 2, 1", -0.5));

        questions.Add(Q(T("CN-C1"), "核外电子的排布遵循的规律不包括：",
            D.Medium, "电子排布遵循能量最低原理、泡利不相容原理和洪特规则，不包括质量守恒定律。",
            "质量守恒定律", "能量最低原理", "泡利不相容原理", "洪特规则", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CN-C2 化学反应 (Chemical Reactions)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-C2"), "下列属于化合反应的是：",
            D.Easy, "2H₂ + O₂ → 2H₂O 是多种物质生成一种物质，属化合反应。",
            "2H₂ + O₂ → 2H₂O", "2H₂O → 2H₂ + O₂",
            "Fe + CuSO₄ → FeSO₄ + Cu", "NaOH + HCl → NaCl + H₂O", -0.8));

        questions.Add(Q(T("CN-C2"), "氧化还原反应的本质是：",
            D.Easy, "氧化还原反应的本质是电子的转移（得失或偏移）。",
            "电子的转移", "氧的得失", "化合价的变化", "能量的释放", -0.5));

        questions.Add(Q(T("CN-C2"), "在反应 Fe₂O₃ + 3CO → 2Fe + 3CO₂ 中，还原剂是：",
            D.Medium, "CO 中碳的化合价从 +2 升高到 +4（被氧化），所以 CO 是还原剂。",
            "CO", "Fe₂O₃", "Fe", "CO₂", 0.2));

        questions.Add(Q(T("CN-C2"), "配平化学方程式 _Al + _O₂ → _Al₂O₃ 的系数为：",
            D.Easy, "4Al + 3O₂ → 2Al₂O₃。系数：4, 3, 2。",
            "4, 3, 2", "2, 3, 1", "1, 1, 1", "3, 2, 1", -0.5));

        questions.Add(Q(T("CN-C2"), "放热反应是指：",
            D.Easy, "反应过程中向环境释放热量的化学反应。",
            "反应过程中释放热量的反应", "反应过程中吸收热量的反应",
            "需要加热才能发生的反应", "只在高温下进行的反应", -0.8));

        questions.Add(Q(T("CN-C2"), "下列反应中，氧化剂是 Cl₂ 的是：",
            D.Medium, "Cl₂ + 2NaBr → 2NaCl + Br₂ 中 Cl₂ 得电子（0→-1）被还原，是氧化剂。",
            "Cl₂ + 2NaBr → 2NaCl + Br₂", "2Na + Cl₂ → 2NaCl",
            "两者都是", "两者都不是", 0.2));

        // ═══════════════════════════════════════════════════════════════
        // CN-C3 化学计量 (Stoichiometry)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-C3"), "阿伏伽德罗常数 Nₐ 约为：",
            D.Easy, "Nₐ ≈ 6.022 × 10²³ /mol。",
            "6.022 × 10²³", "6.022 × 10²²", "6.022 × 10²⁴", "3.01 × 10²³", -1.0));

        questions.Add(Q(T("CN-C3"), "1 mol H₂O 的质量为：",
            D.Easy, "M(H₂O) = 2(1) + 16 = 18 g/mol。1 mol = 18 g。",
            "18 g", "16 g", "2 g", "36 g", -0.8));

        questions.Add(Q(T("CN-C3"), "标况下 1 mol 任何理想气体的体积约为：",
            D.Easy, "标准状况下气体摩尔体积 = 22.4 L/mol。",
            "22.4 L", "11.2 L", "44.8 L", "24.0 L", -0.8));

        questions.Add(Q(T("CN-C3"), "0.5 mol/L 的 NaCl 溶液 200 mL 含 NaCl 的物质的量为：",
            D.Easy, "n = cV = 0.5 × 0.2 = 0.1 mol。",
            "0.1 mol", "0.5 mol", "1 mol", "0.05 mol", -0.5));

        questions.Add(Q(T("CN-C3"), "将 100 mL 1 mol/L 的 HCl 稀释至 500 mL，浓度变为：",
            D.Easy, "c₁V₁ = c₂V₂ → 1 × 100 = c₂ × 500 → c₂ = 0.2 mol/L。",
            "0.2 mol/L", "0.5 mol/L", "5 mol/L", "0.1 mol/L", -0.5));

        questions.Add(Q(T("CN-C3"), "在反应 2H₂ + O₂ → 2H₂O 中，4 g H₂ 完全反应需要 O₂ 的质量为：",
            D.Medium, "4 g H₂ = 2 mol，需 1 mol O₂ = 32 g。",
            "32 g", "16 g", "64 g", "8 g", 0.2));

        // ═══════════════════════════════════════════════════════════════
        // CN-C4 碱金属 (Alkali Metals)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-C4"), "钠保存在煤油中是因为：",
            D.Easy, "钠会与空气中的氧气和水反应，煤油隔绝空气和水。",
            "钠会与空气中的氧气和水反应", "钠在煤油中会溶解",
            "煤油能增强钠的硬度", "钠的密度大于煤油", -0.8));

        questions.Add(Q(T("CN-C4"), "钠在过量氧气中燃烧的产物是：",
            D.Easy, "2Na + O₂ → Na₂O₂（过氧化钠）。",
            "Na₂O₂（过氧化钠）", "Na₂O（氧化钠）", "NaOH", "NaCl", -0.5));

        questions.Add(Q(T("CN-C4"), "碳酸钠和碳酸氢钠的区别方法是：",
            D.Medium, "加热：NaHCO₃ 受热分解放出 CO₂，Na₂CO₃ 受热不分解。",
            "加热——NaHCO₃ 分解，Na₂CO₃ 不分解",
            "加水——只有一种能溶解",
            "颜色不同", "加入 NaCl 溶液", 0.2));

        questions.Add(Q(T("CN-C4"), "焰色反应中，钠的火焰颜色为：",
            D.Easy, "钠的焰色反应呈黄色。",
            "黄色", "紫色", "红色", "绿色", -1.0));

        questions.Add(Q(T("CN-C4"), "碱金属从上到下，金属性：",
            D.Easy, "碱金属从 Li 到 Cs，原子半径增大，失电子能力增强，金属性增强。",
            "逐渐增强", "逐渐减弱", "不变", "先增后减", -0.5));

        questions.Add(Q(T("CN-C4"), "NaOH 的俗名是：",
            D.Easy, "NaOH 俗称烧碱、火碱或苛性钠。",
            "烧碱（火碱）", "纯碱", "小苏打", "生石灰", -1.0));

        // ═══════════════════════════════════════════════════════════════
        // CN-C5 卤素 (Halogens)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-C5"), "氯气的颜色和气味是：",
            D.Easy, "Cl₂ 是黄绿色、有刺激性气味的有毒气体。",
            "黄绿色，刺激性气味", "无色无味", "紫黑色固体", "棕褐色液体", -0.8));

        questions.Add(Q(T("CN-C5"), "卤素单质的氧化性从上到下：",
            D.Easy, "F₂ > Cl₂ > Br₂ > I₂，从上到下氧化性减弱。",
            "依次减弱", "依次增强", "保持不变", "先增后减", -0.5));

        questions.Add(Q(T("CN-C5"), "检验溶液中是否含有 Cl⁻ 的方法是：",
            D.Easy, "加入 AgNO₃ 溶液，生成白色沉淀（AgCl），加稀 HNO₃ 不溶解。",
            "加 AgNO₃ 溶液和稀 HNO₃", "加 BaCl₂ 溶液",
            "加 NaOH 溶液", "石蕊试液", -0.5));

        questions.Add(Q(T("CN-C5"), "将 Cl₂ 通入 KBr 溶液中，溶液变为：",
            D.Medium, "Cl₂ + 2KBr → 2KCl + Br₂，置换出 Br₂ 使溶液变为橙棕色。",
            "橙棕色", "无色", "黄绿色", "紫色", 0.2));

        questions.Add(Q(T("CN-C5"), "HF 与其他氢化物（HCl、HBr、HI）不同的是：",
            D.Medium, "HF 是弱酸（H-F 键很强），而 HCl、HBr、HI 都是强酸。",
            "HF 是弱酸", "HF 是气体", "HF 不能电离", "HF 无毒", 0.3));

        questions.Add(Q(T("CN-C5"), "实验室制备 Cl₂ 的常用反应是：",
            D.Medium, "MnO₂ + 4HCl(浓) → MnCl₂ + Cl₂↑ + 2H₂O。",
            "MnO₂ 与浓盐酸加热", "NaCl 与 H₂SO₄",
            "电解 NaCl 溶液", "加热 KClO₃", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CN-C6 金属及化合物 (Metals & Compounds)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-C6"), "铝与 NaOH 溶液反应生成的气体是：",
            D.Easy, "2Al + 2NaOH + 2H₂O → 2NaAlO₂ + 3H₂↑。",
            "H₂", "O₂", "Cl₂", "CO₂", -0.5));

        questions.Add(Q(T("CN-C6"), "铁在潮湿空气中生锈的主要产物是：",
            D.Easy, "铁锈的主要成分是 Fe₂O₃·xH₂O（水合氧化铁）。",
            "Fe₂O₃·xH₂O", "FeO", "Fe₃O₄", "FeCl₃", -0.5));

        questions.Add(Q(T("CN-C6"), "Fe³⁺ 和 Fe²⁺ 的鉴别方法是：",
            D.Medium, "加 KSCN 溶液：Fe³⁺ 变为血红色，Fe²⁺ 无明显变化。",
            "加 KSCN——Fe³⁺ 变血红色", "加 NaOH——两者均沉淀",
            "加 HCl——产生气泡", "观察颜色——两者均无色", 0.2));

        questions.Add(Q(T("CN-C6"), "Al₂O₃ 是两性氧化物，它能与下列哪些物质反应？",
            D.Medium, "Al₂O₃ 既能与酸反应也能与碱反应（两性）。",
            "酸和碱都能反应", "只能与酸反应",
            "只能与碱反应", "不与酸碱反应", 0.2));

        questions.Add(Q(T("CN-C6"), "铜在潮湿空气中生成的铜绿的化学式是：",
            D.Medium, "铜绿（碱式碳酸铜）：Cu₂(OH)₂CO₃。",
            "Cu₂(OH)₂CO₃", "CuO", "CuCO₃", "Cu(OH)₂", 0.3));

        questions.Add(Q(T("CN-C6"), "下列金属中，活泼性最强的是：",
            D.Easy, "K > Na > Mg > Al > Fe > Cu，钾最活泼。",
            "K", "Fe", "Cu", "Al", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CN-C7 非金属及化合物 (Non-metals & Compounds)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-C7"), "浓 H₂SO₄ 的特性不包括：",
            D.Medium, "浓 H₂SO₄ 有吸水性、脱水性和强氧化性，没有还原性。",
            "还原性", "吸水性", "脱水性", "强氧化性（加热时）", 0.3));

        questions.Add(Q(T("CN-C7"), "SO₂ 能使品红溶液褪色，这是因为：",
            D.Medium, "SO₂ 与品红反应生成不稳定的无色化合物（可逆），加热恢复红色。",
            "SO₂ 与品红结合生成不稳定无色物", "SO₂ 是强氧化剂",
            "SO₂ 是碱性气体", "品红被还原", 0.3));

        questions.Add(Q(T("CN-C7"), "NH₃ 是碱性气体，其水溶液呈碱性是因为：",
            D.Easy, "NH₃ + H₂O ⇌ NH₃·H₂O ⇌ NH₄⁺ + OH⁻，产生 OH⁻。",
            "NH₃ 溶于水产生 OH⁻", "NH₃ 含有 OH 基团",
            "NH₃ 是强碱", "NH₃ 分子极性大", -0.3));

        questions.Add(Q(T("CN-C7"), "工业合成氨的反应条件是：",
            D.Medium, "N₂ + 3H₂ ⇌ 2NH₃。条件：高温（400-500°C）、高压（10-30 MPa）、催化剂（铁）。",
            "高温、高压、铁催化剂", "常温常压", "低温高压", "高温低压", 0.2));

        questions.Add(Q(T("CN-C7"), "硅是重要的半导体材料，硅在自然界中主要以什么形式存在？",
            D.Easy, "硅在自然界主要以 SiO₂（石英/沙子）和硅酸盐的形式存在。",
            "SiO₂ 和硅酸盐", "单质硅", "SiH₄", "SiC", -0.5));

        questions.Add(Q(T("CN-C7"), "CO₂ 通入澄清石灰水变浑浊是因为：",
            D.Easy, "CO₂ + Ca(OH)₂ → CaCO₃↓ + H₂O，生成不溶的 CaCO₃ 沉淀。",
            "生成了不溶的 CaCO₃ 沉淀", "CO₂ 不溶于水",
            "Ca(OH)₂ 分解了", "发生了氧化还原反应", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CN-C8 有机化学基础 (Organic Chemistry)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-C8"), "烷烃的通式为：",
            D.Easy, "烷烃通式 CₙH₂ₙ₊₂。",
            "CₙH₂ₙ₊₂", "CₙH₂ₙ", "CₙH₂ₙ₋₂", "CₙHₙ", -1.0));

        questions.Add(Q(T("CN-C8"), "甲烷的化学式为：",
            D.Easy, "甲烷是最简单的烷烃：CH₄。",
            "CH₄", "C₂H₆", "C₂H₄", "C₂H₂", -1.2));

        questions.Add(Q(T("CN-C8"), "乙烯能使溴水褪色是因为发生了：",
            D.Easy, "CH₂=CH₂ + Br₂ → CH₂BrCH₂Br，加成反应使 Br₂ 被消耗。",
            "加成反应", "取代反应", "消去反应", "氧化反应", -0.5));

        questions.Add(Q(T("CN-C8"), "乙醇的官能团是：",
            D.Easy, "乙醇 C₂H₅OH 含羟基 –OH。",
            "–OH（羟基）", "–COOH（羧基）", "–CHO（醛基）", "C=C（双键）", -0.8));

        questions.Add(Q(T("CN-C8"), "酯化反应是指：",
            D.Easy, "醇和羧酸发生反应生成酯和水。",
            "醇和羧酸生成酯和水", "两个醇脱水", "烯烃加水", "酸和碱中和", -0.5));

        questions.Add(Q(T("CN-C8"), "C₄H₁₀ 有几种同分异构体？",
            D.Easy, "C₄H₁₀ 有 2 种同分异构体：正丁烷和异丁烷。",
            "2 种", "1 种", "3 种", "4 种", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CN-C9 化学平衡 (Chemical Equilibrium)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-C9"), "化学平衡状态的标志是：",
            D.Easy, "正反应速率等于逆反应速率（v正 = v逆），各组分浓度不再改变。",
            "正逆反应速率相等，各组分浓度不变", "反应停止",
            "反应物完全消耗", "只有正反应在进行", -0.8));

        questions.Add(Q(T("CN-C9"), "升高温度对化学平衡的影响是：",
            D.Medium, "升高温度，平衡向吸热方向移动（勒夏特列原理）。",
            "平衡向吸热方向移动", "平衡不移动",
            "平衡向放热方向移动", "取决于压强", 0.3));

        questions.Add(Q(T("CN-C9"), "在 N₂ + 3H₂ ⇌ 2NH₃ 中，增大压强，平衡如何移动？",
            D.Medium, "反应物 4 mol 气体 → 产物 2 mol 气体。增大压强向气体体积减小方向（正向）移动。",
            "向正反应方向移动（生成 NH₃）", "向逆反应方向移动",
            "不移动", "向两个方向同时移动", 0.3));

        questions.Add(Q(T("CN-C9"), "催化剂对化学平衡的影响是：",
            D.Easy, "催化剂同等程度地加快正逆反应速率，不影响平衡位置。",
            "不改变平衡位置", "使平衡向正方向移动",
            "使平衡向逆方向移动", "使平衡常数增大", -0.5));

        questions.Add(Q(T("CN-C9"), "平衡常数 K 只与什么因素有关？",
            D.Medium, "K 只与温度有关，与浓度、压强、催化剂无关。",
            "温度", "浓度", "压强", "催化剂", 0.2));

        questions.Add(Q(T("CN-C9"), "恒温恒容条件下，向平衡体系中充入惰性气体：",
            D.Medium, "恒温恒容下充入惰性气体，各组分分压和浓度不变，平衡不移动。",
            "平衡不移动", "平衡正向移动", "平衡逆向移动", "平衡常数增大", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CN-C10 电化学 (Electrochemistry)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CN-C10"), "原电池中，负极发生的是：",
            D.Easy, "负极：氧化反应（失电子）。正极：还原反应（得电子）。",
            "氧化反应", "还原反应", "没有反应", "中和反应", -0.8));

        questions.Add(Q(T("CN-C10"), "锌铜原电池中，锌是：",
            D.Easy, "锌比铜活泼，锌失电子做负极。",
            "负极", "正极", "不参与反应", "催化剂", -0.8));

        questions.Add(Q(T("CN-C10"), "电解食盐水的主要产物是：",
            D.Medium, "2NaCl + 2H₂O → 2NaOH + H₂↑ + Cl₂↑。",
            "NaOH、H₂ 和 Cl₂", "Na 和 Cl₂",
            "NaCl 和 H₂O", "只有 Cl₂", 0.3));

        questions.Add(Q(T("CN-C10"), "电镀时，镀件应连接的电极是：",
            D.Easy, "电镀时镀件做阴极（接电源负极），镀层金属做阳极。",
            "阴极（连电源负极）", "阳极（连电源正极）",
            "任意一极", "不接电极", -0.5));

        questions.Add(Q(T("CN-C10"), "金属腐蚀中最常见的类型是：",
            D.Medium, "吸氧腐蚀（在中性或弱酸性环境下最常见）。",
            "吸氧腐蚀", "析氢腐蚀", "化学腐蚀", "干燥腐蚀", 0.3));

        questions.Add(Q(T("CN-C10"), "防止金属腐蚀的方法不包括：",
            D.Easy, "增加空气湿度会加速腐蚀。",
            "增加空气湿度", "涂油漆", "镀保护层", "阴极保护法", -0.3));

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
