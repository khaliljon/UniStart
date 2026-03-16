using Microsoft.EntityFrameworkCore;
using UniStart.Domain.Entities;

namespace UniStart.Infrastructure.Data;

/// <summary>
/// Seeds ~120 multiple-choice questions for CSCA Chinese Humanitarian / 中文人文 (8 chapters).
/// Each question has exactly 4 answer options, difficulty level, explanation, and IRT parameters.
/// All questions and options are in Chinese.
/// </summary>
public class CscaChineseHumQuestionSeeder
{
    private readonly UniStartDbContext _context;

    public CscaChineseHumQuestionSeeder(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var humSection = await _context.ExamSections
            .FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Chinese Humanitarian"));
        if (humSection == null) return;

        var existing = await _context.Questions
            .AnyAsync(q => q.Topic.SectionId == humSection.Id);
        if (existing) return;

        var topics = await _context.Topics
            .Where(t => t.SectionId == humSection.Id)
            .ToDictionaryAsync(t => t.Name.Split(' ')[0], t => t.Id);

        int T(string prefix) => topics.TryGetValue(prefix, out var id) ? id : topics.Values.First();

        var questions = new List<Question>();

        // ═══════════════════════════════════════════════════════════════
        // CH1 中国文学 (Chinese Literature)
        // ═══════════════════════════════════════════════════════════════

        // --- CH1.1.1 Classical Chinese Poetry (古诗词) ---
        questions.Add(Q(T("CH1.1.1"), "「床前明月光，疑是地上霜」出自哪位诗人？",
            D.Easy, "这是李白的《静夜思》。",
            "李白", "杜甫", "白居易", "王维", -1.2));

        questions.Add(Q(T("CH1.1.1"), "「春眠不觉晓，处处闻啼鸟」的作者是：",
            D.Easy, "这是孟浩然的《春晓》。",
            "孟浩然", "李白", "杜牧", "王昌龄", -1.0));

        questions.Add(Q(T("CH1.1.1"), "唐代诗人中被称为」诗圣」的是：",
            D.Easy, "杜甫被尊称为」诗圣」，其诗被称为」诗史」。",
            "杜甫", "李白", "白居易", "王维", -0.8));

        questions.Add(Q(T("CH1.1.1"), "「大漠孤烟直，长河落日圆」属于什么诗歌题材？",
            D.Medium, "这是王维描写边塞风光的名句，属于边塞诗。",
            "边塞诗", "田园诗", "山水诗", "咏史诗", 0.2));

        questions.Add(Q(T("CH1.1.1"), "下列哪句诗体现了」借景抒情」的手法？",
            D.Medium, "「感时花溅泪，恨别鸟惊心」中以花鸟表达忧国伤时之情，是借景抒情。",
            "「感时花溅泪，恨别鸟惊心」", "「白日依山尽，黄河入海流」",
            "「飞流直下三千尺」", "「两个黄鹂鸣翠柳」", 0.3));

        // --- CH1.1.2 Modern Chinese Literature (现代文学) ---
        questions.Add(Q(T("CH1.1.2"), "鲁迅的第一篇白话小说是：",
            D.Easy, "《狂人日记》是中国文学史上第一篇白话短篇小说。",
            "《狂人日记》", "《阿Q正传》", "《祝福》", "《孔乙己》", -0.8));

        questions.Add(Q(T("CH1.1.2"), "《骆驼祥子》的作者是：",
            D.Easy, "老舍著有《骆驼祥子》《茶馆》等作品。",
            "老舍", "巴金", "矛盾", "曹禺", -0.5));

        questions.Add(Q(T("CH1.1.2"), "下列哪部作品是巴金的代表作？",
            D.Easy, "巴金的」激流三部曲」：《家》《春》《秋》。",
            "《家》", "《骆驼祥子》", "《围城》", "《子夜》", -0.5));

        // --- CH1.2.1 Idioms and Proverbs (成语与谚语) ---
        questions.Add(Q(T("CH1.2.1"), "成语」画蛇添足」的意思是：",
            D.Easy, "画蛇添足：做多余的事，反而把事情弄坏。",
            "做多余的事反而不好", "形容画技高超",
            "做事非常认真", "速度很快", -0.8));

        questions.Add(Q(T("CH1.2.1"), "「守株待兔」这个成语告诉我们：",
            D.Easy, "守株待兔：不能存有侥幸心理，不劳而获。",
            "不能心存侥幸，坐等好事", "要有耐心等待",
            "兔子跑得快", "树很重要", -0.5));

        questions.Add(Q(T("CH1.2.1"), "成语」塞翁失马」比喻：",
            D.Medium, "塞翁失马，焉知非福。比喻坏事可能变成好事。",
            "坏事可能变成好事", "老人骑马很危险",
            "失去的东西找不回来", "做事要小心谨慎", 0.2));

        questions.Add(Q(T("CH1.2.1"), "「班门弄斧」的意思是：",
            D.Easy, "在行家面前卖弄本领。",
            "在行家面前卖弄本领", "用斧头砍门",
            "班级里学习木工", "做事很有技术", -0.5));

        // --- CH1.2.2 Literary Analysis Techniques (文学分析) ---
        questions.Add(Q(T("CH1.2.2"), "下列哪种不是常见的修辞手法？",
            D.Easy, "演绎推理是逻辑学概念，不是修辞手法。常见修辞有比喻、拟人、排比、夸张等。",
            "演绎推理", "比喻", "排比", "拟人", -0.8));

        questions.Add(Q(T("CH1.2.2"), "「桃花潭水深千尺，不及汪伦送我情」运用了什么修辞手法？",
            D.Easy, "用深千尺的潭水来衬托友情，是夸张和比喻。",
            "夸张", "拟人", "排比", "反问", -0.3));

        // ═══════════════════════════════════════════════════════════════
        // CH2 中国历史 (Chinese History)
        // ═══════════════════════════════════════════════════════════════

        // --- CH2.1.1 Ancient Chinese History (古代史) ---
        questions.Add(Q(T("CH2.1.1"), "中国历史上第一个统一的封建王朝是：",
            D.Easy, "公元前 221 年，秦始皇统一六国建立秦朝。",
            "秦朝", "汉朝", "周朝", "唐朝", -1.0));

        questions.Add(Q(T("CH2.1.1"), "万里长城最初是在哪个朝代开始大规模修建的？",
            D.Easy, "秦始皇统一六国后，连接并扩建了战国时期各国的长城。",
            "秦朝", "汉朝", "明朝", "唐朝", -0.5));

        questions.Add(Q(T("CH2.1.1"), "「丝绸之路」是在哪个朝代开辟的？",
            D.Easy, "西汉时期，张骞出使西域，开辟了丝绸之路。",
            "汉朝（西汉）", "唐朝", "宋朝", "元朝", -0.5));

        questions.Add(Q(T("CH2.1.1"), "科举制度正式确立于哪个朝代？",
            D.Medium, "隋朝开创了科举制度，唐朝进一步发展完善。",
            "隋朝", "唐朝", "宋朝", "明朝", 0.3));

        // --- CH2.1.2 Modern Chinese History (近现代史) ---
        questions.Add(Q(T("CH2.1.2"), "鸦片战争发生于哪一年？",
            D.Easy, "第一次鸦片战争：1840-1842 年。",
            "1840 年", "1860 年", "1894 年", "1911 年", -0.8));

        questions.Add(Q(T("CH2.1.2"), "辛亥革命发生在哪一年？",
            D.Easy, "辛亥革命发生在 1911 年，推翻了清朝统治。",
            "1911 年", "1919 年", "1949 年", "1840 年", -0.5));

        questions.Add(Q(T("CH2.1.2"), "中华人民共和国成立于哪一年？",
            D.Easy, "1949 年 10 月 1 日，中华人民共和国成立。",
            "1949 年", "1945 年", "1950 年", "1939 年", -1.0));

        // --- CH2.2.1 Historical Figures and Events (历史人物与事件) ---
        questions.Add(Q(T("CH2.2.1"), "「四大发明」不包括下列哪项？",
            D.Easy, "四大发明：造纸术、印刷术、火药、指南针。瓷器不在其中。",
            "瓷器", "造纸术", "火药", "指南针", -0.5));

        questions.Add(Q(T("CH2.2.1"), "被称为」书圣」的是：",
            D.Easy, "东晋书法家王羲之被尊称为」书圣」。",
            "王羲之", "颜真卿", "柳公权", "欧阳询", -0.5));

        questions.Add(Q(T("CH2.2.1"), "郑和下西洋发生在哪个朝代？",
            D.Easy, "明朝永乐年间（1405-1433），郑和七次下西洋。",
            "明朝", "宋朝", "元朝", "清朝", -0.3));

        // ═══════════════════════════════════════════════════════════════
        // CH3 中国哲学与思想 (Chinese Philosophy and Thought)
        // ═══════════════════════════════════════════════════════════════

        // --- CH3.1.1 Confucianism (儒家思想) ---
        questions.Add(Q(T("CH3.1.1"), "儒家学派的创始人是：",
            D.Easy, "孔子（公元前 551-479 年）是儒家学派的创始人。",
            "孔子", "孟子", "荀子", "老子", -1.2));

        questions.Add(Q(T("CH3.1.1"), "儒家的核心思想」仁」主要指：",
            D.Easy, "「仁」是儒家最高道德范畴，主要指爱人、关怀他人。",
            "爱人、关怀他人", "服从法律", "追求自然", "追求财富", -0.5));

        questions.Add(Q(T("CH3.1.1"), "「己所不欲，勿施于人」体现了儒家的什么思想？",
            D.Easy, "这句话出自《论语》，体现了」恕」的思想——推己及人。",
            "恕——推己及人", "礼——社会秩序", "义——正义公平", "智——智慧明理", -0.3));

        questions.Add(Q(T("CH3.1.1"), "「四书」不包括下列哪一部？",
            D.Medium, "四书：《大学》《中庸》《论语》《孟子》。《春秋》属于五经。",
            "《春秋》", "《大学》", "《论语》", "《孟子》", 0.3));

        // --- CH3.1.2 Taoism and Buddhism (道家与佛教) ---
        questions.Add(Q(T("CH3.1.2"), "道家学派的创始人是：",
            D.Easy, "老子是道家学派的创始人，著有《道德经》。",
            "老子", "庄子", "孔子", "韩非子", -0.8));

        questions.Add(Q(T("CH3.1.2"), "「无为而治」是哪个学派的核心主张？",
            D.Easy, "道家主张」无为而治」，顺应自然规律。",
            "道家", "儒家", "法家", "墨家", -0.5));

        questions.Add(Q(T("CH3.1.2"), "佛教传入中国大约是在：",
            D.Medium, "佛教大约在西汉末年（公元前后）至东汉初年传入中国。",
            "两汉之际（公元前后）", "春秋战国时期", "唐朝", "宋朝", 0.3));

        // --- CH3.2.1 Modern Chinese Thought (现代思想) ---
        questions.Add(Q(T("CH3.2.1"), "五四运动中提出的两大口号是：",
            D.Easy, "五四运动（1919）提出」民主」和」科学」（德先生和赛先生）。",
            "民主与科学", "自由与平等", "和平与发展", "改革与开放", -0.5));

        questions.Add(Q(T("CH3.2.1"), "新文化运动的主要倡导者不包括：",
            D.Medium, "新文化运动主要倡导者：陈独秀、李大钊、胡适、鲁迅等。孙中山推动的是政治革命。",
            "孙中山", "陈独秀", "胡适", "鲁迅", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CH4 中国文化与艺术 (Chinese Culture and Art)
        // ═══════════════════════════════════════════════════════════════

        // --- CH4.1.1 Traditional Art Forms (传统艺术) ---
        questions.Add(Q(T("CH4.1.1"), "京剧被称为中国的：",
            D.Easy, "京剧被称为中国的」国粹」。",
            "国粹", "国画", "国乐", "国学", -0.8));

        questions.Add(Q(T("CH4.1.1"), "中国书法的基本书体不包括：",
            D.Medium, "基本书体：篆书、隶书、草书、楷书、行书。哥特体是西方字体。",
            "哥特体", "楷书", "草书", "行书", 0.2));

        questions.Add(Q(T("CH4.1.1"), "中国传统绘画的主要工具是：",
            D.Easy, "「文房四宝」中的毛笔和墨是国画的主要工具。",
            "毛笔和墨", "油画笔和颜料", "铅笔和橡皮", "蜡笔和水彩", -0.8));

        // --- CH4.1.2 Festivals and Customs (节日与风俗) ---
        questions.Add(Q(T("CH4.1.2"), "春节是农历的：",
            D.Easy, "春节（Chinese New Year）是农历正月初一。",
            "正月初一", "五月初五", "八月十五", "九月初九", -1.0));

        questions.Add(Q(T("CH4.1.2"), "端午节的传统习俗是：",
            D.Easy, "端午节（五月初五）传统习俗有吃粽子和赛龙舟。",
            "吃粽子、赛龙舟", "吃月饼、赏月",
            "吃汤圆、猜灯谜", "登高、赏菊花", -0.5));

        questions.Add(Q(T("CH4.1.2"), "中秋节的传统食品是：",
            D.Easy, "中秋节（八月十五）的传统食品是月饼。",
            "月饼", "粽子", "汤圆", "饺子", -0.8));

        questions.Add(Q(T("CH4.1.2"), "下列哪个节日与纪念屈原有关？",
            D.Easy, "端午节（农历五月初五）与纪念爱国诗人屈原有关。",
            "端午节", "清明节", "重阳节", "中秋节", -0.5));

        // --- CH4.2.1 Chinese Cinema and Music (电影与音乐) ---
        questions.Add(Q(T("CH4.2.1"), "中国传统音乐中的」五声音阶」包括哪五个音？",
            D.Medium, "五声音阶：宫、商、角、徵、羽。",
            "宫、商、角、徵、羽", "do、re、mi、sol、la",
            "金、木、水、火、土", "仁、义、礼、智、信", 0.3));

        questions.Add(Q(T("CH4.2.1"), "中国最早的电影是：",
            D.Medium, "1905 年拍摄的京剧纪录片《定军山》被认为是中国最早的电影。",
            "《定军山》（1905）", "《歌女红牡丹》（1931）",
            "《马路天使》（1937）", "《渔光曲》（1934）", 0.5));

        // ═══════════════════════════════════════════════════════════════
        // CH5 中国社会 (Chinese Society)
        // ═══════════════════════════════════════════════════════════════

        // --- CH5.1.1 Chinese Education System (教育体系) ---
        questions.Add(Q(T("CH5.1.1"), "中国的义务教育为多少年？",
            D.Easy, "中国实行九年义务教育（小学 6 年 + 初中 3 年）。",
            "9 年", "6 年", "12 年", "15 年", -1.0));

        questions.Add(Q(T("CH5.1.1"), "中国的高考全称是：",
            D.Easy, "高考全称」普通高等学校招生全国统一考试」。",
            "普通高等学校招生全国统一考试", "中等学校入学考试",
            "研究生入学考试", "公务员录用考试", -0.5));

        questions.Add(Q(T("CH5.1.1"), "中国最知名的两所大学常被合称为：",
            D.Easy, "北京大学和清华大学常被合称为」北大清华」或」清北」。",
            "北大清华", "复旦交大", "武大华科", "南大浙大", -0.5));

        // --- CH5.1.2 Social Issues and Current Events (社会问题) ---
        questions.Add(Q(T("CH5.1.2"), "中国目前面临的主要社会问题不包括：",
            D.Medium, "人口过剩已不再是主要问题，当前面临的是老龄化和出生率下降。",
            "人口急剧增长", "老龄化", "城乡差距", "环境污染", 0.3));

        questions.Add(Q(T("CH5.1.2"), "「一带一路」倡议指的是：",
            D.Medium, "「一带一路」即」丝绸之路经济带」和」21世纪海上丝绸之路」。",
            "丝绸之路经济带和 21 世纪海上丝绸之路",
            "中国国内高速公路网络",
            "中国高铁发展规划",
            "中国互联网发展战略", 0.3));

        // --- CH5.2.1 Chinese Geography and Regions (地理与区域) ---
        questions.Add(Q(T("CH5.2.1"), "中国面积最大的省级行政区是：",
            D.Easy, "新疆维吾尔自治区面积约 166 万平方公里，是中国面积最大的省级行政区。",
            "新疆", "西藏", "内蒙古", "青海", -0.5));

        questions.Add(Q(T("CH5.2.1"), "中国的母亲河是指：",
            D.Easy, "黄河被称为中华民族的」母亲河」。",
            "黄河", "长江", "珠江", "黑龙江", -0.8));

        questions.Add(Q(T("CH5.2.1"), "中国最长的河流是：",
            D.Easy, "长江全长约 6300 公里，是中国最长、世界第三长的河流。",
            "长江", "黄河", "珠江", "松花江", -0.8));

        // ═══════════════════════════════════════════════════════════════
        // CH6 学术写作（人文）(Academic Writing — Humanities)
        // ═══════════════════════════════════════════════════════════════

        // --- CH6.1.1 Essay Structure in Chinese (论文结构) ---
        questions.Add(Q(T("CH6.1.1"), "中文议论文的基本结构是：",
            D.Easy, "议论文基本结构：提出问题（引论）→ 分析问题（本论）→ 解决问题（结论）。",
            "引论、本论、结论", "开头、中间、结尾",
            "起因、经过、结果", "时间、地点、人物", -0.5));

        questions.Add(Q(T("CH6.1.1"), "论文中」论点」指的是：",
            D.Easy, "论点是作者对所论述问题的看法和主张。",
            "作者的观点和主张", "引用的事实材料",
            "论证的方法", "文章的结构", -0.5));

        questions.Add(Q(T("CH6.1.1"), "下列哪项不属于常见的论证方法？",
            D.Medium, "常见论证方法：举例、道理、对比、比喻论证等。」抒情」不是论证方法。",
            "抒情", "举例论证", "对比论证", "道理论证", 0.2));

        // --- CH6.1.2 Argumentative Writing (议论文写作) ---
        questions.Add(Q(T("CH6.1.2"), "议论文中」论据」的作用是：",
            D.Easy, "论据用来支撑和证明论点。",
            "支撑和证明论点", "提出新的问题",
            "描写具体场景", "抒发个人情感", -0.5));

        questions.Add(Q(T("CH6.1.2"), "下列哪句适合作为议论文的开头？",
            D.Medium, "直接提出论点或用引言引出话题适合做议论文开头。",
            "「随着科技发展，人工智能正深刻改变我们的生活」",
            "「今天天气真好啊」",
            "「从前有一座山」",
            "「亲爱的朋友们，大家好」", 0.2));

        // --- CH6.2.1 Formal and Informal Register (正式与非正式用语) ---
        questions.Add(Q(T("CH6.2.1"), "下列哪个词语属于正式用语？",
            D.Easy, "「因此」是书面正式语体。」所以」较口语化。",
            "因此", "所以", "然后", "就是", -0.5));

        questions.Add(Q(T("CH6.2.1"), "学术写作中应避免使用的是：",
            D.Easy, "学术写作应避免口语化表达，保持客观严谨。",
            "口语化表达和网络用语", "专业术语",
            "准确的数据", "引用权威文献", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CH7 阅读理解（人文）(Reading Comprehension — Humanities)
        // ═══════════════════════════════════════════════════════════════

        // --- CH7.1.1 News and Media Articles (新闻阅读) ---
        questions.Add(Q(T("CH7.1.1"), "新闻标题的主要作用是：",
            D.Easy, "标题概括新闻的核心内容，吸引读者。",
            "概括核心内容，吸引读者", "表达记者个人观点",
            "提供详细信息", "总结读者评论", -0.5));

        questions.Add(Q(T("CH7.1.1"), "新闻报道的」5W1H「不包括：",
            D.Easy, "5W1H：Who, What, When, Where, Why, How。不包括」How much「。",
            "How much（花费多少）", "What（什么事）",
            "When（什么时候）", "Why（为什么）", -0.3));

        questions.Add(Q(T("CH7.1.1"), "新闻写作中，」倒金字塔」结构是指：",
            D.Medium, "最重要的信息放在最前面，次要信息依次排后。",
            "最重要的信息在前，次要信息在后",
            "按时间顺序叙述",
            "结尾是最重要的部分",
            "开头和结尾同样重要", 0.2));

        // --- CH7.1.2 Academic Texts (学术文本) ---
        questions.Add(Q(T("CH7.1.2"), "阅读学术文本时，首先应关注的是：",
            D.Easy, "先阅读摘要和结论，了解文章主旨和核心观点。",
            "摘要和结论", "参考文献", "脚注", "致谢", -0.5));

        questions.Add(Q(T("CH7.1.2"), "学术文本中」综述」是指：",
            D.Medium, "综述是对某一领域已有文献和研究成果的系统梳理和评述。",
            "对已有研究的系统梳理和评述",
            "作者的原创实验",
            "对某个事件的新闻报道",
            "个人的学习笔记", 0.2));

        // --- CH7.2.1 Opinion and Editorial Analysis (评论分析) ---
        questions.Add(Q(T("CH7.2.1"), "评论文章与新闻报道的主要区别在于：",
            D.Easy, "评论文章表达作者的主观看法和分析，新闻报道强调客观事实。",
            "评论表达主观看法，新闻报道客观事实",
            "评论篇幅更短",
            "新闻报道不需要事实依据",
            "评论不需要论据支撑", -0.5));

        questions.Add(Q(T("CH7.2.1"), "在分析评论文章时，最重要的是找出：",
            D.Easy, "找出作者的核心观点（论点）是分析评论文章的关键。",
            "作者的核心观点", "文章的字数",
            "作者的年龄", "发表日期", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CH8 考试策略 (Exam Strategies)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("CH8.1.1"), "CSCA 中文人文考试时间为 90 分钟，共 80 题，平均每题用时约：",
            D.Easy, "90 分钟 ÷ 80 题 = 1.125 分钟/题 ≈ 67.5 秒/题。",
            "约 1 分钟", "约 2 分钟", "约 30 秒", "约 5 分钟", -0.8));

        questions.Add(Q(T("CH8.1.1"), "考试时间不够时，最佳策略是：",
            D.Easy, "在剩余时间内先做有把握的题目，不确定的题目快速排除后猜答。",
            "先做有把握的题，不确定的排除后猜答",
            "按顺序做完所有题目",
            "随机选择答案",
            "放弃所有未完成的题目", -0.5));

        questions.Add(Q(T("CH8.1.2"), "选择题中排除法的核心是：",
            D.Easy, "先排除明显错误的选项，提高剩余选项中猜对的概率。",
            "排除明显错误选项，提高猜对概率",
            "选最长的选项",
            "选看起来最不同的选项",
            "永远选 C", -0.5));

        questions.Add(Q(T("CH8.1.2"), "遇到不确定的题目时，以下哪种做法最有效？",
            D.Medium, "先用排除法缩小范围，标记后继续，有时间再回来检查。",
            "排除法缩小范围后标记，有时间再检查",
            "花很长时间反复思考",
            "直接跳过不作答",
            "放弃整个考试", 0.2));

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
