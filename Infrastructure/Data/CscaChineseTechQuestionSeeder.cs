using Microsoft.EntityFrameworkCore;
using UniStart.Domain.Entities;

namespace UniStart.Infrastructure.Data;

/// <summary>
/// Seeds ~120 multiple-choice questions for CSCA Chinese Technical / 中文技术 (8 chapters).
/// Each question has exactly 4 answer options, difficulty level, explanation, and IRT parameters.
/// All questions and options are in Chinese.
/// </summary>
public class CscaChineseTechQuestionSeeder
{
    private readonly UniStartDbContext _context;

    public CscaChineseTechQuestionSeeder(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var techSection = await _context.ExamSections
            .FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Chinese Technical"));
        if (techSection == null) return;

        var existing = await _context.Questions
            .AnyAsync(q => q.Topic.SectionId == techSection.Id);
        if (existing) return;

        var topics = await _context.Topics
            .Where(t => t.SectionId == techSection.Id)
            .ToDictionaryAsync(t => t.Name.Split(' ')[0], t => t.Id);

        int T(string prefix) => topics.TryGetValue(prefix, out var id) ? id : topics.Values.First();

        var questions = new List<Question>();

        // ═══════════════════════════════════════════════════════════════
        // CT1 科学词汇 (Scientific Vocabulary)
        // ═══════════════════════════════════════════════════════════════

        // --- CT1.1.1 Mathematics & Physics Terminology (数学与物理术语) ---
        questions.Add(Q(T("CT1.1.1"), "「微积分」的英文是：",
            D.Easy, "微积分 = Calculus。",
            "Calculus", "Algebra", "Geometry", "Statistics", -0.8));

        questions.Add(Q(T("CT1.1.1"), "「加速度」的定义是：",
            D.Easy, "加速度是速度随时间的变化率。",
            "速度随时间的变化率", "位移随时间的变化率",
            "力与质量的比值", "动量的变化", -0.5));

        questions.Add(Q(T("CT1.1.1"), "下列哪个是向量（矢量）？",
            D.Easy, "力有大小和方向，是矢量。质量、温度、时间是标量。",
            "力", "质量", "温度", "时间", -0.5));

        questions.Add(Q(T("CT1.1.1"), "「函数」的数学符号通常写作：",
            D.Easy, "函数通常写作 f(x)，表示 x 的函数。",
            "f(x)", "Σx", "∫x", "Δx", -0.8));

        // --- CT1.1.2 Chemistry & Biology Terminology (化学与生物术语) ---
        questions.Add(Q(T("CT1.1.2"), "「催化剂」在反应中的作用是：",
            D.Easy, "催化剂改变反应速率，但自身不被消耗。",
            "加快反应速率，自身不被消耗", "提供反应所需的能量",
            "增加产物的质量", "改变反应产物的种类", -0.5));

        questions.Add(Q(T("CT1.1.2"), "DNA 的中文全称是：",
            D.Easy, "DNA = 脱氧核糖核酸。",
            "脱氧核糖核酸", "核糖核酸", "腺嘌呤核苷酸", "蛋白质", -0.5));

        questions.Add(Q(T("CT1.1.2"), "「光合作用」的产物是：",
            D.Easy, "光合作用：6CO₂ + 6H₂O → C₆H₁₂O₆ + 6O₂。",
            "葡萄糖和氧气", "二氧化碳和水", "蛋白质和脂肪", "淀粉和氮气", -0.3));

        // --- CT1.2.1 Technology and Computing Terms (技术与计算机术语) ---
        questions.Add(Q(T("CT1.2.1"), "「算法」的定义是：",
            D.Easy, "算法是解决特定问题的一系列有限步骤。",
            "解决问题的有限步骤序列", "一种编程语言",
            "计算机硬件", "数据库系统", -0.5));

        questions.Add(Q(T("CT1.2.1"), "CPU 的中文全称是：",
            D.Easy, "CPU = Central Processing Unit = 中央处理器。",
            "中央处理器", "随机存取存储器", "图形处理器", "输入输出设备", -0.8));

        questions.Add(Q(T("CT1.2.1"), "「人工智能」的英文缩写是：",
            D.Easy, "人工智能 = Artificial Intelligence = AI。",
            "AI", "IT", "AR", "VR", -1.0));

        // --- CT1.2.2 Engineering Terms (工程术语) ---
        questions.Add(Q(T("CT1.2.2"), "「应力」等于：",
            D.Medium, "应力σ = F/A，单位面积上受到的力。",
            "力除以截面积", "力乘以距离", "质量乘以加速度", "力乘以时间", 0.2));

        questions.Add(Q(T("CT1.2.2"), "工程图中的」三视图」包括：",
            D.Easy, "三视图：正视图（主视图）、俯视图、侧视图（左视图）。",
            "正视图、俯视图、侧视图", "透视图、轴测图、剖面图",
            "平面图、立面图、鸟瞰图", "正投影、斜投影、中心投影", -0.3));

        // ═══════════════════════════════════════════════════════════════
        // CT2 阅读科技文本 (Reading Scientific Texts)
        // ═══════════════════════════════════════════════════════════════

        // --- CT2.1.1 Reading Scientific Papers (科学论文阅读) ---
        questions.Add(Q(T("CT2.1.1"), "科技论文的」摘要」（Abstract）的作用是：",
            D.Easy, "摘要简要概括论文的研究目的、方法、结果和结论。",
            "概括研究目的、方法、结果和结论", "列出所有参考文献",
            "详细描述实验过程", "对前人研究进行综述", -0.5));

        questions.Add(Q(T("CT2.1.1"), "科技论文中」方法」部分主要描述：",
            D.Easy, "方法部分描述实验设计、材料和具体操作步骤。",
            "实验设计和操作步骤", "研究背景和意义",
            "数据分析结果", "对结果的讨论和解释", -0.3));

        questions.Add(Q(T("CT2.1.1"), "论文中引用其他文献的主要目的是：",
            D.Easy, "引用文献为观点提供依据，同时尊重前人成果。",
            "为观点提供依据并尊重前人成果",
            "增加论文的页数",
            "展示作者读了很多书",
            "填充论文的空白", -0.5));

        // --- CT2.1.2 Reading Graphs and Data (图表与数据阅读) ---
        questions.Add(Q(T("CT2.1.2"), "折线图最适合用来表示：",
            D.Easy, "折线图适合表示数据随时间的变化趋势。",
            "数据随时间的变化趋势", "各部分占总体的比例",
            "不同类别之间的比较", "数据的分布情况", -0.3));

        questions.Add(Q(T("CT2.1.2"), "饼图（扇形图）最适合用来表示：",
            D.Easy, "饼图直观地表示各部分占总体的比例关系。",
            "各部分占总体的比例", "数据随时间的变化趋势",
            "两个变量之间的相关性", "数据的频率分布", -0.3));

        questions.Add(Q(T("CT2.1.2"), "散点图用来分析：",
            D.Easy, "散点图用于分析两个变量之间的相关关系。",
            "两个变量之间的相关性", "数据的时间趋势",
            "各类别之间的占比", "数据的中位数", -0.3));

        // --- CT2.2.1 Textbook Structure (教科书结构) ---
        questions.Add(Q(T("CT2.2.1"), "教科书中」目录」的作用是：",
            D.Easy, "目录列出章节标题和页码，帮助读者快速找到内容。",
            "列出章节标题和页码，便于查找", "提供所有公式的汇总",
            "列出所有练习题的答案", "介绍作者的研究背景", -0.5));

        questions.Add(Q(T("CT2.2.1"), "阅读理科教科书的推荐方法是：",
            D.Medium, "SQ3R 法（浏览→提问→阅读→复述→复习）是科学阅读的有效方法。",
            "浏览-提问-阅读-复述-复习", "从头到尾逐字阅读",
            "只看公式和定理", "只做课后习题", 0.2));

        // --- CT2.2.2 Technical Manuals (技术手册阅读) ---
        questions.Add(Q(T("CT2.2.2"), "技术手册中」FAQ「的意思是：",
            D.Easy, "FAQ = Frequently Asked Questions = 常见问题解答。",
            "常见问题解答", "快速参考指南", "错误代码列表", "软件更新日志", -0.8));

        questions.Add(Q(T("CT2.2.2"), "阅读技术文档时，遇到不理解的术语应该：",
            D.Easy, "查阅术语表（Glossary）或上下文推断含义。",
            "查阅术语表或根据上下文推断", "跳过不管",
            "用日常用语替换", "翻译成英文", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CT3 工程与技术 (Engineering & Technology)
        // ═══════════════════════════════════════════════════════════════

        // --- CT3.1.1 Basic Engineering Concepts (基础工程概念) ---
        questions.Add(Q(T("CT3.1.1"), "工程设计中」可行性分析」的目的是：",
            D.Easy, "评估项目在技术、经济等方面是否可行。",
            "评估项目是否可行", "确定项目的最终方案",
            "编写项目报告", "招募项目人员", -0.5));

        questions.Add(Q(T("CT3.1.1"), "」CAD「是什么的缩写？",
            D.Easy, "CAD = Computer-Aided Design = 计算机辅助设计。",
            "计算机辅助设计", "计算机辅助制造", "计算机辅助教学", "计算机辅助测试", -0.5));

        questions.Add(Q(T("CT3.1.1"), "工程项目中」原型」的作用是：",
            D.Easy, "原型用于在正式生产前验证设计方案的可行性。",
            "在正式生产前验证设计方案", "直接用于客户交付",
            "代替所有测试", "仅用于展示外观", -0.3));

        // --- CT3.1.2 Information Technology (信息技术) ---
        questions.Add(Q(T("CT3.1.2"), "下列哪个是编程语言？",
            D.Easy, "Python 是编程语言。HTML 是标记语言，SQL 是查询语言，CSS 是样式表。",
            "Python", "HTML", "SQL", "CSS", -0.5));

        questions.Add(Q(T("CT3.1.2"), "「云计算」的核心概念是：",
            D.Easy, "云计算通过网络按需提供计算资源和服务。",
            "通过网络按需提供计算资源", "在本地服务器上处理数据",
            "使用超级计算机", "将数据存储在U盘中", -0.3));

        questions.Add(Q(T("CT3.1.2"), "下列哪个不是操作系统？",
            D.Easy, "Photoshop 是图像处理软件，不是操作系统。Windows、Linux、macOS 都是。",
            "Photoshop", "Windows", "Linux", "macOS", -0.5));

        // --- CT3.2.1 Electrical and Mechanical Basics (电气与机械基础) ---
        questions.Add(Q(T("CT3.2.1"), "电阻的单位是：",
            D.Easy, "电阻的 SI 单位是欧姆（Ω）。",
            "欧姆（Ω）", "伏特（V）", "安培（A）", "瓦特（W）", -0.8));

        questions.Add(Q(T("CT3.2.1"), "欧姆定律的表达式是：",
            D.Easy, "欧姆定律：U = IR（电压 = 电流 × 电阻）。",
            "U = IR", "P = UI", "F = ma", "E = mc²", -0.5));

        questions.Add(Q(T("CT3.2.1"), "杠杆平衡条件是：",
            D.Easy, "杠杆平衡条件：F₁ × L₁ = F₂ × L₂。",
            "F₁ × L₁ = F₂ × L₂", "F₁ + L₁ = F₂ + L₂",
            "F₁ / L₁ = F₂ / L₂", "F₁ - L₁ = F₂ - L₂", -0.3));

        // --- CT3.2.2 Environmental Engineering (环境工程) ---
        questions.Add(Q(T("CT3.2.2"), "「可再生能源」不包括：",
            D.Easy, "煤炭属于化石燃料（不可再生）。太阳能、风能、水力均可再生。",
            "煤炭", "太阳能", "风能", "水力发电", -0.5));

        questions.Add(Q(T("CT3.2.2"), "「碳中和」的含义是：",
            D.Medium, "碳中和：CO₂排放量与吸收量平衡，实现净零排放。",
            "CO₂排放量与吸收量平衡", "完全不排放CO₂",
            "只使用碳基燃料", "减少碳的生产", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CT4 学术写作（STEM）(Academic Writing — STEM)
        // ═══════════════════════════════════════════════════════════════

        // --- CT4.1.1 Lab Report Structure (实验报告结构) ---
        questions.Add(Q(T("CT4.1.1"), "实验报告的标准结构顺序是：",
            D.Easy, "标准结构：目的→原理→实验步骤→数据记录→分析讨论→结论。",
            "目的→原理→步骤→数据→分析→结论", "结论→分析→数据→步骤→原理→目的",
            "步骤→数据→目的→结论", "原理→结论→步骤→数据", -0.5));

        questions.Add(Q(T("CT4.1.1"), "实验报告中」误差分析」的作用是：",
            D.Medium, "分析实验结果与理论值偏差的原因，评估实验的可靠性。",
            "分析结果偏差的原因并评估可靠性",
            "证明实验完全正确",
            "列出所有使用过的仪器",
            "描述实验的理论背景", 0.2));

        // --- CT4.1.2 Scientific Writing Style (科技写作风格) ---
        questions.Add(Q(T("CT4.1.2"), "科技写作中应使用的语态是：",
            D.Easy, "科技写作通常使用被动语态（如」实验结果表明……「），保持客观性。",
            "多用被动语态，保持客观", "使用第一人称活泼表达",
            "使用感叹句增强语气", "使用网络流行语", -0.3));

        questions.Add(Q(T("CT4.1.2"), "数据描述中，」标准差」衡量的是：",
            D.Medium, "标准差衡量数据的离散程度（波动大小）。",
            "数据的离散程度", "数据的平均值",
            "数据的最大值", "数据的中位数", 0.3));

        // --- CT4.2.1 Technical Presentation (技术报告呈现) ---
        questions.Add(Q(T("CT4.2.1"), "技术报告中，图表标题应放在：",
            D.Easy, "表格标题在上方，图片标题在下方是国际通用惯例。",
            "表格标题在上方，图片标题在下方",
            "全部在上方",
            "全部在下方",
            "不需要标题", -0.3));

        questions.Add(Q(T("CT4.2.1"), "引用数据时，以下哪种做法最规范？",
            D.Easy, "引用数据时应注明来源和年份，保证可追溯性。",
            "注明数据来源和年份", "不需要说明来源",
            "只写」数据来源于网络」", "用大概的数字即可", -0.3));

        // ═══════════════════════════════════════════════════════════════
        // CT5 听力理解 (Listening Comprehension)
        // ═══════════════════════════════════════════════════════════════

        // --- CT5.1.1 Academic Lectures (学术讲座) ---
        questions.Add(Q(T("CT5.1.1"), "听学术讲座时，最有效的笔记方法是：",
            D.Easy, "记录关键词和核心概念，用自己的语言简要概括。",
            "记录关键词，用自己的话概括", "逐字记录所有内容",
            "只听不记", "录音后不整理", -0.5));

        questions.Add(Q(T("CT5.1.1"), "讲座中出现」换句话说」这一信号词时，通常意味着：",
            D.Easy, "「换句话说」表示即将用不同方式解释同一观点。",
            "即将用不同方式解释同一观点", "话题即将转变",
            "要举一个例子", "讲座即将结束", -0.3));

        questions.Add(Q(T("CT5.1.1"), "听力理解中，」预测」策略是指：",
            D.Medium, "根据上下文和已有信息，预测接下来可能出现的内容。",
            "根据上下文预测即将出现的内容",
            "提前看听力原文",
            "猜测所有答案",
            "不听直接做题", 0.2));

        // --- CT5.1.2 Technical Discussions (技术讨论) ---
        questions.Add(Q(T("CT5.1.2"), "听技术讨论时，遇到不熟悉的术语应：",
            D.Easy, "根据上下文推断含义，记下术语后继续听。",
            "根据上下文推断，记下后继续听",
            "立刻停下来查字典",
            "放弃听这段内容",
            "用自己认为的意思替代", -0.3));

        questions.Add(Q(T("CT5.1.2"), "小组技术讨论中，表达不同意见时最恰当的方式是：",
            D.Easy, "礼貌表达不同看法并给出理由是专业讨论的基本礼仪。",
            "「我理解您的观点，但我认为……因为……」",
            "「你说的完全不对」",
            "「随便吧，都行」",
            "保持沉默不发言", -0.5));

        // --- CT5.2.1 Numbers and Data in Audio (听力中的数字与数据) ---
        questions.Add(Q(T("CT5.2.1"), "听力中」大约三千万」用数字表示为：",
            D.Easy, "三千万 = 30,000,000 = 3 × 10⁷。",
            "30,000,000", "3,000,000", "300,000,000", "3,000", -0.5));

        questions.Add(Q(T("CT5.2.1"), "听到」增长了百分之十五」时，增长率是：",
            D.Easy, "「百分之十五」= 15%。",
            "15%", "1.5%", "50%", "150%", -0.8));

        // ═══════════════════════════════════════════════════════════════
        // CT6 STEM语法 (Grammar for STEM)
        // ═══════════════════════════════════════════════════════════════

        // --- CT6.1.1 Sentence Structure (句式结构) ---
        questions.Add(Q(T("CT6.1.1"), "下列哪个句子是被动句？",
            D.Easy, "被动句：用」被」字或」由……「结构表明主语是动作的承受者。",
            "「实验数据被仔细分析了」", "「研究人员分析了数据」",
            "「数据非常重要」", "「我们需要更多数据」", -0.5));

        questions.Add(Q(T("CT6.1.1"), "科技中文中，复杂句通常用什么来连接？",
            D.Easy, "科技中文使用关联词对连接复杂句，如」因为……所以……「。",
            "关联词对（因为……所以……）", "逗号",
            "感叹号", "省略号", -0.3));

        questions.Add(Q(T("CT6.1.1"), "「虽然实验条件有限，但结果仍然可靠」中的关联词表示：",
            D.Easy, "「虽然……但……「表示转折关系。",
            "转折关系", "因果关系", "递进关系", "并列关系", -0.3));

        // --- CT6.1.2 Common Connectors (常用连接词) ---
        questions.Add(Q(T("CT6.1.2"), "表示因果关系的连接词是：",
            D.Easy, "「因此」表示因果关系。",
            "因此", "但是", "或者", "而且", -0.8));

        questions.Add(Q(T("CT6.1.2"), "「此外」在句子中的作用是：",
            D.Easy, "「此外」用于补充说明，表示递进关系。",
            "补充说明（递进）", "表示转折", "表示因果", "表示选择", -0.3));

        // --- CT6.2.1 Number Expressions (数字表达) ---
        questions.Add(Q(T("CT6.2.1"), "「温度升高了 ΔT = 15°C「用中文完整表述为：",
            D.Easy, "正确读法：温度升高了 15 摄氏度。",
            "温度升高了十五摄氏度", "温度是十五度",
            "温度高了十五", "温差为十五华氏度", -0.5));

        questions.Add(Q(T("CT6.2.1"), "科技中文中，大数」1.2 × 10⁸「的口语读法是：",
            D.Medium, "1.2 × 10⁸ = 1.2 亿。",
            "一点二亿", "一百二十万", "十二亿", "一千二百万", 0.2));

        // ═══════════════════════════════════════════════════════════════
        // CT7 练习套题 (Practice Sets)
        // ═══════════════════════════════════════════════════════════════

        // --- CT7.1.1 Vocabulary Practice (词汇练习) ---
        questions.Add(Q(T("CT7.1.1"), "「合金」是指：",
            D.Easy, "合金是两种或两种以上金属（或金属与非金属）的混合物。",
            "两种以上金属或金属与非金属的混合物",
            "纯金属",
            "金属氧化物",
            "金属的化合物", -0.5));

        questions.Add(Q(T("CT7.1.1"), "「半导体」的导电性介于：",
            D.Easy, "半导体的导电能力在导体和绝缘体之间。",
            "导体和绝缘体之间", "超导体和导体之间",
            "绝缘体和真空之间", "液体和固体之间", -0.3));

        questions.Add(Q(T("CT7.1.1"), "「生态系统」由什么组成？",
            D.Easy, "生态系统 = 生物群落 + 非生物环境（阳光、水、土壤等）。",
            "生物群落和非生物环境", "只有动物和植物",
            "只有微生物", "只有水和空气", -0.3));

        // --- CT7.1.2 Reading Practice (阅读练习) ---
        questions.Add(Q(T("CT7.1.2"), "科技文章中」综上所述」通常出现在：",
            D.Easy, "「综上所述」是总结性词语，通常出现在文章结尾或段落总结处。",
            "文章结尾或段落总结处", "文章开头",
            "实验描述部分", "参考文献前", -0.5));

        questions.Add(Q(T("CT7.1.2"), "阅读科技文本时，以下哪种策略最有效？",
            D.Easy, "先通读标题和各段首句，掌握文章结构和主要论点。",
            "先读标题和段首句，掌握结构", "逐字阅读全文",
            "只看图表不看文字", "从最后一段开始读", -0.3));

        // --- CT7.2.1 Integrated Skills (综合技能练习) ---
        questions.Add(Q(T("CT7.2.1"), "将」该实验在室温下进行」改为被动句：",
            D.Easy, "主语」实验」是动作」进行」的承受者，可改为被字句。",
            "「该实验是在室温下被进行的」", "「室温进行了该实验」",
            "「在室温下实验进行了」", "「该实验室温下进行」", -0.3));

        questions.Add(Q(T("CT7.2.1"), "以下哪个是正确的单位换算？",
            D.Easy, "1 km = 1000 m。",
            "1 km = 1000 m", "1 km = 100 m",
            "1 km = 10000 m", "1 km = 10 m", -0.8));

        // ═══════════════════════════════════════════════════════════════
        // CT8 考试策略 (Exam Strategies)
        // ═══════════════════════════════════════════════════════════════

        // --- CT8.1.1 Time Management (时间管理) ---
        questions.Add(Q(T("CT8.1.1"), "CSCA 中文技术考试时间为 90 分钟，共 80 题，建议的做题策略是：",
            D.Easy, "先做简单题确保得分，难题留到后面，最后检查。",
            "先易后难，最后检查", "按顺序逐题做完",
            "先做最难的题", "每题花相同时间", -0.5));

        questions.Add(Q(T("CT8.1.1"), "如果一道题超过 2 分钟还没有解决，最佳做法是：",
            D.Easy, "先标记后跳过，保证其他题不受影响，最后回来处理。",
            "标记后跳过，最后回来处理", "继续思考直到解出",
            "随便选一个然后放弃", "向监考老师求助", -0.3));

        // --- CT8.1.2 Answering Techniques (答题技巧) ---
        questions.Add(Q(T("CT8.1.2"), "选择题中，如果两个选项意思相反，正确答案很可能是：",
            D.Medium, "两个相反选项中，正确答案很可能是其中之一。",
            "两个相反选项之一", "两者都不是",
            "其他两个选项之一", "无法判断", 0.3));

        questions.Add(Q(T("CT8.1.2"), "技术类选择题中，带有」总是」「一定」「绝对」等绝对词的选项：",
            D.Easy, "技术领域中绝对表述通常不够严谨，这类选项往往是错误的。",
            "通常是错误选项", "通常是正确选项",
            "无法判断", "与答案无关", -0.3));

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
