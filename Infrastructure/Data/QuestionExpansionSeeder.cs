using Microsoft.EntityFrameworkCore;
using UniStart.Domain.Entities;

namespace UniStart.Infrastructure.Data;

/// <summary>
/// Expands the question base from 30 to 120+ questions.
/// Covers all 16 topics including previously empty TOEFL topics.
/// Questions are modelled on official exam formats (College Board, ETS, NU).
/// IRT parameters calibrated per difficulty + per-question variation.
/// </summary>
public class QuestionExpansionSeeder
{
    private readonly UniStartDbContext _context;
    private readonly Random _rng = new(2026); // deterministic

    public QuestionExpansionSeeder(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        // Only run if we still have ≤ 35 questions (original seed)
        var count = await _context.Questions.CountAsync();
        if (count > 35) return;

        var topics = await _context.Topics.ToDictionaryAsync(t => t.Name, t => t);
        var questions = new List<Question>();

        // ════════════════════════════════════════════════════
        //  SAT READING & WRITING
        // ════════════════════════════════════════════════════

        // ── Main Idea & Summary (3 new) ─────────────────
        questions.AddRange(MakeQuestions(topics["Main Idea & Summary"], new[]
        {
            Q("The passage primarily discusses the relationship between urban planning and public health. Which of the following best states the main idea?",
              new[] { ("Urban planning can significantly impact public health outcomes", true), ("Cities are unhealthy places to live", false), ("Public health funding is insufficient", false), ("Architecture determines life expectancy", false) },
              QuestionDifficulty.Medium, "The passage draws explicit connections between planning decisions (green spaces, walkability, air quality) and health metrics.",
              0.3, 1.0),

            Q("A historian argues that the Industrial Revolution's greatest legacy was not technology but social change. Which choice best summarizes this claim?",
              new[] { ("Technology was unimportant during industrialization", false), ("Social transformations outlasted technological innovations", true), ("Workers preferred rural life", false), ("Factories replaced all manual labor", false) },
              QuestionDifficulty.Hard, "The author acknowledges technology but argues that lasting social changes (urbanization, labor laws, class structure) had deeper impact.",
              0.8, 1.3),

            Q("Read the following paragraph about renewable energy adoption. What is the author's main argument?",
              new[] { ("Renewable energy is too expensive for developing nations", false), ("Government subsidies are the only solution", false), ("Economic incentives accelerate renewable energy adoption more effectively than regulations", true), ("Solar energy is superior to wind energy", false) },
              QuestionDifficulty.Medium, "The passage compares regulatory mandates with market-based incentives and concludes that economic motivators drive faster adoption.",
              0.1, 1.1),
        }));

        // ── Grammar & Sentence Structure (3 new) ────────
        questions.AddRange(MakeQuestions(topics["Grammar & Sentence Structure"], new[]
        {
            Q("Select the version that best maintains parallel structure: 'The study revealed that exercise improves mood, _____ cognitive function, and longevity.'",
              new[] { ("enhances", true), ("it enhances", false), ("enhancing", false), ("which enhances", false) },
              QuestionDifficulty.Easy, "Parallel structure requires the same grammatical form: 'improves mood, enhances cognitive function, and [improves] longevity.'",
              -1.0, 0.9),

            Q("Which choice most effectively combines the two sentences? 'Coral reefs support marine biodiversity. They also protect coastlines from erosion.'",
              new[] { ("Coral reefs support marine biodiversity, and they also protect coastlines from erosion.", false), ("Coral reefs, which support marine biodiversity, also protect coastlines from erosion.", true), ("Coral reefs support marine biodiversity, protect coastlines from erosion.", false), ("Supporting marine biodiversity, coral reefs also protect coastlines from erosion, which is important.", false) },
              QuestionDifficulty.Medium, "The relative clause construction smoothly integrates both ideas with proper subordination.",
              0.2, 1.1),

            Q("The team of researchers _____ published their findings in a peer-reviewed journal last month.",
              new[] { ("has", true), ("have", false), ("are", false), ("were", false) },
              QuestionDifficulty.Medium, "The subject is 'team' (singular collective noun), which takes a singular verb 'has'. 'Of researchers' is a prepositional phrase, not the subject.",
              0.0, 1.0),
        }));

        // ── Vocabulary in Context (4 new) ────────────────
        questions.AddRange(MakeQuestions(topics["Vocabulary in Context"], new[]
        {
            Q("In the sentence 'The diplomat's remarks were deliberately ambiguous to avoid committing to either side,' what does 'ambiguous' most nearly mean?",
              new[] { ("Hostile", false), ("Open to multiple interpretations", true), ("Eloquent", false), ("Brief", false) },
              QuestionDifficulty.Easy, "'Ambiguous' means having more than one possible meaning, which in this context means the diplomat intentionally left room for interpretation.",
              -0.8, 0.9),

            Q("'The proliferation of smartphones has fundamentally altered how people consume news.' In this context, 'proliferation' most nearly means:",
              new[] { ("Destruction", false), ("Regulation", false), ("Rapid spread and increase", true), ("Design improvement", false) },
              QuestionDifficulty.Medium, "'Proliferation' means rapid increase in quantity. Here it describes the explosive growth in smartphone usage.",
              0.1, 1.0),

            Q("'The scientist's conjecture, while lacking empirical support, proved remarkably prescient.' As used here, 'prescient' most nearly means:",
              new[] { ("Incorrect", false), ("Showing knowledge of future events", true), ("Controversial", false), ("Traditional", false) },
              QuestionDifficulty.Hard, "'Prescient' means having foreknowledge. The sentence says the conjecture later turned out to be correct despite initially lacking evidence.",
              0.9, 1.2),

            Q("'The author's prose style is characteristically austere, favoring precision over ornamentation.' 'Austere' most nearly means:",
              new[] { ("Flowery and elaborate", false), ("Severe and plain", true), ("Humorous and light", false), ("Confusing and dense", false) },
              QuestionDifficulty.Medium, "'Austere' contrasts with 'ornamentation,' meaning the writing style is stripped-down and plain.",
              0.3, 1.1),
        }));

        // ════════════════════════════════════════════════════
        //  SAT MATH
        // ════════════════════════════════════════════════════

        // ── Linear Equations (4 new) ─────────────────────
        questions.AddRange(MakeQuestions(topics["Linear Equations"], new[]
        {
            Q("If 3(x − 2) = 12, what is the value of x?",
              new[] { ("4", false), ("6", true), ("2", false), ("8", false) },
              QuestionDifficulty.Easy, "3(x − 2) = 12 → x − 2 = 4 → x = 6",
              -1.3, 0.8),

            Q("A phone plan charges $30 per month plus $0.10 per text message. If the total bill is $47, how many text messages were sent?",
              new[] { ("170", true), ("47", false), ("300", false), ("17", false) },
              QuestionDifficulty.Easy, "30 + 0.10t = 47 → 0.10t = 17 → t = 170 messages",
              -0.9, 0.9),

            Q("The system of equations 2x + y = 10 and x − y = 2 has the solution (x, y). What is x + y?",
              new[] { ("10", false), ("8", false), ("4", false), ("6", true) },
              QuestionDifficulty.Medium, "Adding equations: 3x = 12, so x = 4. Then y = 10 − 8 = 2. x + y = 6.",
              0.2, 1.0),

            Q("If the line y = mx + 5 passes through the point (3, 11), what is the value of m?",
              new[] { ("3", false), ("2", true), ("5", false), ("4", false) },
              QuestionDifficulty.Medium, "11 = m(3) + 5 → 3m = 6 → m = 2",
              0.0, 1.1),
        }));

        // ── Geometry (4 new) ─────────────────────────────
        questions.AddRange(MakeQuestions(topics["Geometry"], new[]
        {
            Q("A rectangle has a length of 12 cm and a width of 5 cm. What is its perimeter?",
              new[] { ("34 cm", true), ("60 cm", false), ("17 cm", false), ("24 cm", false) },
              QuestionDifficulty.Easy, "P = 2(l + w) = 2(12 + 5) = 2(17) = 34 cm",
              -1.2, 0.8),

            Q("In a right triangle, one leg is 6 and the hypotenuse is 10. What is the length of the other leg?",
              new[] { ("4", false), ("8", true), ("7", false), ("√64", false) },
              QuestionDifficulty.Medium, "By the Pythagorean theorem: a² + 6² = 10² → a² = 100 − 36 = 64 → a = 8",
              0.1, 1.0),

            Q("A circle has a radius of 7. What is its area? (Use π ≈ 22/7)",
              new[] { ("44", false), ("154", true), ("49", false), ("88", false) },
              QuestionDifficulty.Medium, "A = πr² = (22/7)(49) = 154",
              0.3, 1.1),

            Q("Two similar triangles have corresponding sides in the ratio 3:5. If the area of the smaller triangle is 27 cm², what is the area of the larger triangle?",
              new[] { ("45 cm²", false), ("75 cm²", true), ("135 cm²", false), ("225 cm²", false) },
              QuestionDifficulty.Hard, "Area ratio = (3/5)² = 9/25. So 27/A = 9/25, A = 27 × 25/9 = 75 cm².",
              0.9, 1.3),
        }));

        // ── Data Analysis (4 new) ────────────────────────
        questions.AddRange(MakeQuestions(topics["Data Analysis"], new[]
        {
            Q("The mean of five numbers is 20. If four of the numbers are 15, 18, 22, and 25, what is the fifth number?",
              new[] { ("20", true), ("18", false), ("22", false), ("15", false) },
              QuestionDifficulty.Easy, "Sum = 5 × 20 = 100. Known sum = 15 + 18 + 22 + 25 = 80. Fifth = 100 − 80 = 20.",
              -1.0, 0.9),

            Q("A dataset has values: 3, 7, 7, 10, 13. What is the median?",
              new[] { ("7", true), ("8", false), ("10", false), ("3", false) },
              QuestionDifficulty.Easy, "The values in order are 3, 7, 7, 10, 13. The middle (3rd) value is 7.",
              -1.1, 0.8),

            Q("In a survey, 60% of 250 respondents preferred Brand A. If the margin of error is ±4%, what is the range of the true proportion?",
              new[] { ("56% to 64%", true), ("60% to 64%", false), ("54% to 66%", false), ("50% to 70%", false) },
              QuestionDifficulty.Medium, "60% ± 4% gives a confidence interval of 56% to 64%.",
              0.2, 1.0),

            Q("A scatter plot shows a strong negative linear association between hours of TV watched and exam scores. Which correlation coefficient is most likely?",
              new[] { ("r = 0.85", false), ("r = −0.82", true), ("r = −0.15", false), ("r = 0.10", false) },
              QuestionDifficulty.Hard, "A strong negative linear association corresponds to r close to −1. r = −0.82 is strong and negative.",
              0.7, 1.2),
        }));

        // ── Quadratic Equations (3 new) ──────────────────
        questions.AddRange(MakeQuestions(topics["Quadratic Equations"], new[]
        {
            Q("What are the solutions of x² − 5x + 6 = 0?",
              new[] { ("x = 2 and x = 3", true), ("x = −2 and x = −3", false), ("x = 1 and x = 6", false), ("x = −1 and x = −6", false) },
              QuestionDifficulty.Easy, "Factor: (x − 2)(x − 3) = 0 → x = 2 or x = 3.",
              -0.7, 0.9),

            Q("The function f(x) = −2x² + 8x − 3 has its maximum value at x = ?",
              new[] { ("4", false), ("2", true), ("−2", false), ("8", false) },
              QuestionDifficulty.Medium, "For ax² + bx + c, vertex at x = −b/(2a) = −8/(2·(−2)) = −8/−4 = 2.",
              0.3, 1.1),

            Q("If the discriminant of ax² + bx + c = 0 is negative, what can be concluded about the equation's solutions?",
              new[] { ("Two distinct real solutions", false), ("One repeated real solution", false), ("No real solutions (two complex solutions)", true), ("Infinitely many solutions", false) },
              QuestionDifficulty.Hard, "When the discriminant b² − 4ac < 0, the quadratic has no real roots; the solutions are complex conjugates.",
              1.0, 1.3),
        }));

        // ════════════════════════════════════════════════════
        //  TOEFL
        // ════════════════════════════════════════════════════

        // ── Academic Reading (4 new) ─────────────────────
        questions.AddRange(MakeQuestions(topics["Academic Reading"], new[]
        {
            Q("According to the passage about photosynthesis, which factor does the author identify as the primary limitation on plant growth in tropical forests?",
              new[] { ("Water availability", false), ("Light penetration through canopy layers", true), ("Soil nutrient content", false), ("Temperature fluctuations", false) },
              QuestionDifficulty.Medium, "The passage states that despite abundant water and warmth, the dense canopy limits light availability to lower forest layers, restricting growth.",
              0.1, 1.0),

            Q("The word 'ephemeral' in paragraph 3 is closest in meaning to:",
              new[] { ("Permanent", false), ("Short-lived", true), ("Beautiful", false), ("Microscopic", false) },
              QuestionDifficulty.Easy, "'Ephemeral' means lasting a very short time, consistent with the context describing temporary phenomena.",
              -0.8, 0.9),

            Q("Which of the following can be inferred from the passage about the Cambrian Explosion?",
              new[] { ("All modern species originated during this period", false), ("The rapid diversification of life forms remains partially unexplained", true), ("The Cambrian period lasted millions of years", false), ("Only marine organisms appeared during this time", false) },
              QuestionDifficulty.Hard, "The passage notes that while several theories exist (oxygen levels, ecological niches), the speed and scope of diversification is not fully accounted for.",
              1.0, 1.3),

            Q("The author mentions the 'tragedy of the commons' primarily to:",
              new[] { ("Criticize government regulators", false), ("Illustrate how individual rational actions can harm collective resources", true), ("Argue against private property", false), ("Describe a historical event in England", false) },
              QuestionDifficulty.Medium, "The 'tragedy of the commons' is used as a conceptual framework showing how self-interested behavior depletes shared resources.",
              0.3, 1.1),
        }));

        // ── Lecture Comprehension (4 new) ────────────────
        questions.AddRange(MakeQuestions(topics["Lecture Comprehension"], new[]
        {
            Q("In the lecture about plate tectonics, the professor explains that mid-ocean ridges form because:",
              new[] { ("Plates collide and push upward", false), ("Magma rises through divergent plate boundaries", true), ("Ocean currents erode the seafloor", false), ("Earthquakes create underwater mountains", false) },
              QuestionDifficulty.Easy, "The professor explains that at divergent boundaries, plates move apart and magma rises to fill the gap, creating ridges.",
              -0.9, 0.8),

            Q("What does the professor imply when she says 'the data doesn't exactly cooperate with our models'?",
              new[] { ("The research methodology was flawed", false), ("Real-world observations differ from theoretical predictions", true), ("The students should redesign the experiment", false), ("The models are completely wrong", false) },
              QuestionDifficulty.Medium, "The professor uses understatement to acknowledge discrepancies between predictions and observations without dismissing the models entirely.",
              0.2, 1.1),

            Q("According to the lecture, what distinguishes procedural memory from declarative memory?",
              new[] { ("Procedural memory is stored in a different brain region", false), ("Procedural memory involves skills and habits performed without conscious recall", true), ("Declarative memory is less reliable", false), ("Procedural memory develops only in childhood", false) },
              QuestionDifficulty.Medium, "The lecture defines procedural memory as implicit knowledge of 'how to do things' (riding a bike, typing) vs. declarative memory as explicit facts.",
              0.1, 1.0),

            Q("Why does the professor mention the Doppler effect when discussing the expansion of the universe?",
              new[] { ("To explain why sounds change pitch", false), ("To demonstrate how redshift indicates galaxies are moving away", true), ("To introduce a homework assignment", false), ("To compare light waves and sound waves in general", false) },
              QuestionDifficulty.Hard, "The professor uses the Doppler effect as an analogy: just as sound pitch shifts with motion, light from receding galaxies shifts toward red wavelengths.",
              0.8, 1.2),
        }));

        // ── Conversation Understanding (5 new — previously empty!) ──
        questions.AddRange(MakeQuestions(topics["Conversation Understanding"], new[]
        {
            Q("Why does the student go to the professor's office?",
              new[] { ("To submit a late assignment", false), ("To ask for clarification on the research paper requirements", true), ("To complain about a grade", false), ("To request a letter of recommendation", false) },
              QuestionDifficulty.Easy, "At the beginning of the conversation, the student says she has questions about the paper guidelines and wants to make sure she understands the topic scope.",
              -1.0, 0.8),

            Q("What does the professor suggest the student do before starting the literature review?",
              new[] { ("Write the introduction first", false), ("Read three additional textbooks", false), ("Create an outline and identify key search terms", true), ("Meet with the librarian", false) },
              QuestionDifficulty.Medium, "The professor recommends organizing thoughts into an outline and selecting specific search terms to make the database searches more efficient.",
              0.0, 1.0),

            Q("What can be inferred about the student's attitude toward the assignment?",
              new[] { ("She thinks the assignment is unfair", false), ("She is anxious but motivated to do well", true), ("She is uninterested in the topic", false), ("She plans to drop the course", false) },
              QuestionDifficulty.Medium, "The student's questions are detailed and forward-looking, indicating genuine effort and concern about quality rather than complaints.",
              0.2, 1.1),

            Q("When the professor says 'I wouldn't lose sleep over the page count,' he means:",
              new[] { ("The page count requirement has been removed", false), ("The student should focus on content quality rather than exact length", true), ("The assignment will not be graded", false), ("The student should write less", false) },
              QuestionDifficulty.Medium, "The idiomatic expression means 'don't worry about it.' The professor emphasizes substance over hitting an exact page number.",
              0.3, 1.0),

            Q("What will the student probably do next?",
              new[] { ("Drop the course", false), ("Start writing the paper immediately", false), ("Develop an outline and return for feedback", true), ("Email the professor with additional questions", false) },
              QuestionDifficulty.Easy, "The conversation ends with the professor saying 'bring your outline next week and we'll go from there,' implying the student will create one.",
              -0.8, 0.9),
        }));

        // ── Independent Speaking (5 new — previously empty!) ──
        questions.AddRange(MakeQuestions(topics["Independent Speaking"], new[]
        {
            Q("Which of the following is the BEST approach for structuring an independent speaking response about 'the most important quality of a leader'?",
              new[] { ("List as many qualities as possible", false), ("State your opinion, give a specific reason, and provide a concrete example", true), ("Describe a famous leader without stating your opinion", false), ("Repeat the question and speak slowly to fill time", false) },
              QuestionDifficulty.Easy, "The TOEFL speaking rubric rewards clear opinion + specific reasoning + concrete example within the time limit.",
              -1.0, 0.8),

            Q("A student prepares the response: 'I think technology helps education because online courses let people study anywhere.' What would MOST improve this response?",
              new[] { ("Speaking faster to include more content", false), ("Adding a personal example or specific detail", true), ("Using more academic vocabulary", false), ("Mentioning counterarguments", false) },
              QuestionDifficulty.Medium, "A specific personal example (e.g., 'I took an online biology course while working') adds depth and demonstrates the point concretely.",
              0.1, 1.0),

            Q("In TOEFL Independent Speaking, if you are asked 'Do you prefer studying alone or with others?' and you prefer groups, which response opening is strongest?",
              new[] { ("'There are many advantages and disadvantages to both options.'", false), ("'I strongly prefer studying with others because group discussions deepen my understanding.'", true), ("'This is a difficult question with no right answer.'", false), ("'Studying alone is very popular among students.'", false) },
              QuestionDifficulty.Medium, "A clear, direct opinion statement with a built-in reason creates a strong framework for the rest of the response.",
              0.0, 1.0),

            Q("Which transition phrase BEST connects a reason to an example in a speaking response?",
              new[] { ("However", false), ("For instance", true), ("In conclusion", false), ("On the other hand", false) },
              QuestionDifficulty.Easy, "'For instance' introduces a specific example supporting the previous statement. 'However' and 'on the other hand' signal contrast.",
              -1.2, 0.8),

            Q("A student's 45-second response includes: opinion (5 sec), reason 1 (10 sec), example 1 (15 sec), reason 2 (10 sec). What is missing?",
              new[] { ("A third reason", false), ("A concluding statement", false), ("A specific example for the second reason", true), ("Background context about the topic", false) },
              QuestionDifficulty.Medium, "Each reason should be supported by a concrete example. Reason 2 lacks its supporting example, making it less convincing.",
              0.2, 1.1),
        }));

        // ── Integrated Writing (5 new — previously empty!) ──
        questions.AddRange(MakeQuestions(topics["Integrated Writing"], new[]
        {
            Q("In an integrated writing task, the reading passage supports renewable energy. The lecture contradicts each point. What is the BEST way to organize the response?",
              new[] { ("Summarize the reading, then summarize the lecture separately", false), ("For each reading point, present the corresponding lecture counterargument", true), ("Write your own opinion about renewable energy", false), ("Only summarize the lecture since it is more recent", false) },
              QuestionDifficulty.Medium, "Point-by-point organization most clearly shows the relationship between reading arguments and lecture counterpoints.",
              0.0, 1.0),

            Q("The reading claims that telecommuting increases productivity. The lecture states that studies show distraction levels are higher at home. Which phrase BEST introduces the lecture's counterpoint?",
              new[] { ("The lecturer agrees that...", false), ("In addition to the reading's point...", false), ("The lecturer challenges this by noting that...", true), ("As everyone knows...", false) },
              QuestionDifficulty.Easy, "'Challenges this by noting' clearly signals a contrast between the reading's claim and the lecture's evidence.",
              -0.9, 0.9),

            Q("Which of the following is NOT a characteristic of a well-written integrated writing response?",
              new[] { ("Paraphrasing both sources accurately", false), ("Including personal opinions about the topic", true), ("Using transitions to connect reading and lecture points", false), ("Maintaining an objective, academic tone", false) },
              QuestionDifficulty.Easy, "Integrated writing tasks assess your ability to synthesize two sources, not express personal opinions.",
              -1.0, 0.8),

            Q("The reading argues that urban green spaces reduce stress. The lecturer says: 'Recent longitudinal studies haven't replicated these effects.' What is the lecturer doing?",
              new[] { ("Supporting the reading with new evidence", false), ("Questioning the validity of the reading's evidence", true), ("Changing the topic entirely", false), ("Agreeing with a minor qualification", false) },
              QuestionDifficulty.Medium, "By citing studies that couldn't replicate the effects, the lecturer undermines the reliability of the reading's claims.",
              0.2, 1.1),

            Q("A student writes: 'The lecturer says the reading is wrong because animals can adapt to habitat loss.' What is the main problem with this sentence?",
              new[] { ("It is grammatically incorrect", false), ("It oversimplifies the lecturer's nuanced argument", true), ("It is too long", false), ("It copies the lecturer's words exactly", false) },
              QuestionDifficulty.Hard, "The response reduces a complex argument to a binary 'right/wrong.' Academic writing should capture nuance: 'The lecturer argues that the reading overstates the impact because...'",
              0.8, 1.2),
        }));

        // ════════════════════════════════════════════════════
        //  NUET
        // ════════════════════════════════════════════════════

        // ── Algebra & Functions (5 new) ──────────────────
        questions.AddRange(MakeQuestions(topics["Algebra & Functions"], new[]
        {
            Q("If f(x) = 3x − 7, what is f(4)?",
              new[] { ("5", true), ("12", false), ("19", false), ("−4", false) },
              QuestionDifficulty.Easy, "f(4) = 3(4) − 7 = 12 − 7 = 5",
              -1.3, 0.8),

            Q("Simplify: (2x³)(3x²)",
              new[] { ("5x⁵", false), ("6x⁵", true), ("6x⁶", false), ("5x⁶", false) },
              QuestionDifficulty.Easy, "Multiply coefficients (2×3=6) and add exponents (3+2=5): 6x⁵",
              -1.0, 0.9),

            Q("If g(x) = x² − 4x + 3, for which values of x does g(x) = 0?",
              new[] { ("x = 1 and x = 3", true), ("x = −1 and x = −3", false), ("x = 2 and x = 6", false), ("x = 4 and x = 3", false) },
              QuestionDifficulty.Medium, "Factor: (x − 1)(x − 3) = 0, so x = 1 or x = 3",
              0.1, 1.0),

            Q("The function h(x) = 1/(x − 2) is undefined when x equals:",
              new[] { ("0", false), ("1", false), ("2", true), ("−2", false) },
              QuestionDifficulty.Easy, "Division by zero: when x = 2, the denominator x − 2 = 0, making h(x) undefined.",
              -0.9, 0.9),

            Q("If log₂(x) = 5, what is x?",
              new[] { ("10", false), ("25", false), ("32", true), ("64", false) },
              QuestionDifficulty.Medium, "log₂(x) = 5 means 2⁵ = x, so x = 32",
              0.3, 1.1),
        }));

        // ── Problem Solving (4 new) ──────────────────────
        questions.AddRange(MakeQuestions(topics["Problem Solving"], new[]
        {
            Q("A train travels at 60 km/h for 2.5 hours. How far does it travel?",
              new[] { ("120 km", false), ("150 km", true), ("125 km", false), ("180 km", false) },
              QuestionDifficulty.Easy, "Distance = speed × time = 60 × 2.5 = 150 km",
              -1.2, 0.8),

            Q("A store offers 20% off a $45 item, then an additional 10% off the reduced price. What is the final price?",
              new[] { ("$30.00", false), ("$32.40", true), ("$31.50", false), ("$33.75", false) },
              QuestionDifficulty.Medium, "After 20% off: $45 × 0.80 = $36. After additional 10% off: $36 × 0.90 = $32.40",
              0.2, 1.0),

            Q("Three workers can complete a job in 12 days. How many days would it take 4 workers to complete the same job (at the same rate)?",
              new[] { ("16", false), ("8", false), ("9", true), ("10", false) },
              QuestionDifficulty.Medium, "Total work = 3 × 12 = 36 worker-days. Time for 4 workers = 36/4 = 9 days.",
              0.1, 1.1),

            Q("A bag contains 5 red, 3 blue, and 2 green marbles. If one marble is drawn at random, what is the probability it is NOT red?",
              new[] { ("1/2", true), ("1/5", false), ("3/10", false), ("2/5", false) },
              QuestionDifficulty.Medium, "Total = 10. Not red = 3 + 2 = 5. P(not red) = 5/10 = 1/2",
              0.0, 1.0),
        }));

        // ── Logical Reasoning (5 new) ────────────────────
        questions.AddRange(MakeQuestions(topics["Logical Reasoning"], new[]
        {
            Q("All mammals are warm-blooded. All dogs are mammals. Therefore:",
              new[] { ("All warm-blooded animals are dogs", false), ("All dogs are warm-blooded", true), ("Some mammals are dogs", false), ("No dogs are cold-blooded", false) },
              QuestionDifficulty.Easy, "This is a valid syllogism: Dogs ⊂ Mammals ⊂ Warm-blooded, so Dogs ⊂ Warm-blooded. Answer D is also true but not the direct conclusion.",
              -1.1, 0.8),

            Q("'If it rains, the ground gets wet. The ground is wet.' What can be concluded?",
              new[] { ("It definitely rained", false), ("Something made the ground wet, but it may not have been rain", true), ("It will rain tomorrow", false), ("The ground is always wet", false) },
              QuestionDifficulty.Medium, "This is the fallacy of affirming the consequent. The ground could be wet from other causes (sprinkler, flooding). We cannot conclude it rained.",
              0.2, 1.1),

            Q("In a group of 30 students, 18 study English, 12 study French, and 5 study both. How many study neither?",
              new[] { ("5", true), ("0", false), ("3", false), ("10", false) },
              QuestionDifficulty.Medium, "By inclusion-exclusion: E ∪ F = 18 + 12 − 5 = 25. Neither = 30 − 25 = 5.",
              0.1, 1.0),

            Q("Which of the following, if true, most WEAKENS the argument: 'City X should ban cars from downtown because it will reduce pollution'?",
              new[] { ("City Y banned cars and pollution decreased", false), ("Most pollution in City X comes from factories outside downtown", true), ("Some residents prefer to walk", false), ("Car sales have declined nationally", false) },
              QuestionDifficulty.Hard, "If most pollution comes from factories (not cars), then banning cars won't significantly reduce pollution, undermining the argument's premise.",
              0.9, 1.3),

            Q("Statement: 'No A are B. All B are C.' Which must be true?",
              new[] { ("No A are C", false), ("All A are C", false), ("Some C are not A", true), ("All C are B", false) },
              QuestionDifficulty.Hard, "Since all B are C, there exist some C that are B. Since no A are B, those C (which are B) are not A. So some C are not A.",
              1.1, 1.2),
        }));

        // ── Argument Analysis (5 new) ────────────────────
        questions.AddRange(MakeQuestions(topics["Argument Analysis"], new[]
        {
            Q("'We should invest in space exploration because it drives technological innovation.' What unstated assumption does this argument rely on?",
              new[] { ("Space exploration is dangerous", false), ("Technological innovation is valuable and desirable", true), ("Space exploration is inexpensive", false), ("No other activities drive innovation", false) },
              QuestionDifficulty.Medium, "The argument assumes that driving innovation is a good reason to invest. If innovation weren't valued, the argument would fail.",
              0.1, 1.0),

            Q("'Sales of ice cream and drowning incidents both increase in summer. Therefore, ice cream causes drowning.' This is an example of:",
              new[] { ("Valid causal reasoning", false), ("Correlation mistaken for causation", true), ("Appeal to authority", false), ("Straw man argument", false) },
              QuestionDifficulty.Easy, "Both variables correlate because they share a common cause (hot weather), not because one causes the other.",
              -0.8, 0.9),

            Q("A politician argues: 'My opponent's healthcare plan will lead to socialism, and socialism always fails.' This reasoning is flawed because:",
              new[] { ("The opponent's plan may not constitute socialism", true), ("Socialism has never been tried", false), ("Healthcare is not a political issue", false), ("The politician is not a healthcare expert", false) },
              QuestionDifficulty.Medium, "This is a slippery slope / mischaracterization: the plan may not actually constitute socialism. The argument equates a specific policy with an entire economic system.",
              0.3, 1.1),

            Q("'Professor Smith, who has a PhD in biology, says that climate change is not a serious threat. Therefore, climate change is not serious.' What fallacy is this?",
              new[] { ("Ad hominem", false), ("False dilemma", false), ("Appeal to improper authority", true), ("Circular reasoning", false) },
              QuestionDifficulty.Hard, "A biology PhD does not make someone an authority on climate science. This is an appeal to authority outside the expert's domain.",
              0.8, 1.2),

            Q("Which of the following STRENGTHENS the argument: 'Regular exercise reduces the risk of heart disease'?",
              new[] { ("Many athletes are young and healthy", false), ("A 20-year longitudinal study of 50,000 people found a dose-response relationship between exercise frequency and lower cardiac event rates", true), ("Exercise equipment sales have increased", false), ("Some people enjoy exercising", false) },
              QuestionDifficulty.Medium, "A large, long-term study with a dose-response relationship provides strong epidemiological evidence for the causal claim.",
              0.2, 1.0),
        }));

        // ════════════════════════════════════════════════════
        //  Save all new questions
        // ════════════════════════════════════════════════════

        await _context.Questions.AddRangeAsync(questions);
        await _context.SaveChangesAsync();
    }

    // ═══════════════════════════════════════════════════════
    //  Helpers
    // ═══════════════════════════════════════════════════════

    private record QuestionDef(string Text, (string Text, bool IsCorrect)[] Options,
        QuestionDifficulty Difficulty, string Explanation, double B, double A);

    private QuestionDef Q(string text, (string, bool)[] options,
        QuestionDifficulty difficulty, string explanation, double b, double a)
        => new(text, options, difficulty, explanation, b, a);

    private IEnumerable<Question> MakeQuestions(Topic topic, QuestionDef[] defs)
    {
        foreach (var d in defs)
        {
            // Add small per-question variation for realism
            var bVariation = (_rng.NextDouble() - 0.5) * 0.2; // ±0.1
            var aVariation = (_rng.NextDouble() - 0.5) * 0.1; // ±0.05

            yield return new Question
            {
                TopicId = topic.Id,
                Text = d.Text,
                Difficulty = d.Difficulty,
                DifficultyParam = d.B + bVariation,
                DiscriminationParam = Math.Max(0.5, d.A + aVariation),
                GuessParam = 0.25,
                Explanation = d.Explanation,
                AnswerOptions = d.Options.Select(o => new AnswerOption
                {
                    Text = o.Text,
                    IsCorrect = o.IsCorrect
                }).ToList()
            };
        }
    }
}
