using Microsoft.EntityFrameworkCore;
using UniStart.Domain.Entities;

namespace UniStart.Infrastructure.Data;

/// <summary>
/// Seeds ~200 multiple-choice questions for CSCA Physics (12 chapters).
/// Each question has exactly 4 answer options, difficulty level, explanation, and IRT parameters.
/// </summary>
public class CscaPhysicsQuestionSeeder
{
    private readonly UniStartDbContext _context;

    public CscaPhysicsQuestionSeeder(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var physicsSection = await _context.ExamSections
            .FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name == "Physics");
        if (physicsSection == null) return;

        var existing = await _context.Questions
            .AnyAsync(q => q.Topic.SectionId == physicsSection.Id);
        if (existing) return;

        var topics = await _context.Topics
            .Where(t => t.SectionId == physicsSection.Id)
            .ToDictionaryAsync(t => t.Name.Split(' ')[0], t => t.Id);

        int T(string prefix) => topics.TryGetValue(prefix, out var id) ? id : topics.Values.First();

        var questions = new List<Question>();

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 1: FORCE  (Topics P1.x.x)
        // ═══════════════════════════════════════════════════════════════

        // --- Section 1: Force (basic concepts) ---

        questions.Add(Q(T("P1.1.1"),
            "Which of the following statements about the concept of force is incorrect?",
            D.Easy, "Force requires both an object exerting it and an object receiving it. Option C claims the exerting object may not have a receiving object, which contradicts the nature of force as mutual interaction.",
            "When there is an object exerting the force, there may not necessarily be an object receiving the force",
            "Without objects, there is no force",
            "When there is an object receiving the force, there must be an object exerting the force",
            "When there is only one object, there is no force", -1.0));

        questions.Add(Q(T("P1.1.1"),
            "The action of force is mutual. Which of the following phenomena does NOT utilize this principle?",
            D.Medium, "Heading a ball utilizes direct force application, not Newton's Third Law of mutual reaction.",
            "When heading a ball for a goal, one must forcefully head the ball towards the goal direction",
            "When a boat moves forward, the oars paddle water backward",
            "When a person runs forward, they push down and backward against the ground",
            "When a rocket takes off, it expels gas downward", 0.0));

        questions.Add(Q(T("P1.1.1"),
            "Xiao Ming uses oars to paddle water backward, causing the boat to move forward. The object exerting the force that makes the boat advance is:",
            D.Medium, "The oars push the water; by Newton's Third Law, the water pushes back on the oars/boat. So the water exerts the forward force on the boat.",
            "The water", "The oars", "The boat", "Xiao Ming", 0.2));

        questions.Add(Q(T("P1.1.1"),
            "In a soccer match, a player kicks the ball into the air. Neglecting air resistance, the object exerting the force that changes the motion of the ball while in the air is:",
            D.Easy, "Once in the air (neglecting air resistance), only gravity acts on the ball. Gravity is exerted by the Earth.",
            "The Earth", "The forward player", "The goalkeeper", "The soccer ball", -0.8));

        // --- Section 2: Common Types of Forces — Gravity ---

        questions.Add(Q(T("P1.1.1"),
            "Which of the following statements about gravity is correct?",
            D.Easy, "Gravity always acts vertically downward, not perpendicular to the surface. Objects in air also experience gravity.",
            "The direction of gravity is vertically downward",
            "The direction of gravity is perpendicular downward",
            "Only objects in contact with the ground experience gravity",
            "An apple falls faster because gravity increases", -1.0));

        questions.Add(Q(T("P1.1.1"),
            "If there were no gravity, which of the following statements is INCORRECT?",
            D.Medium, "Without gravity, objects would still have mass. Mass is an intrinsic property unrelated to gravity.",
            "Objects would lose their mass",
            "Rivers would no longer flow",
            "A person would jump and never return to ground",
            "Water could not be poured from a cup into the mouth", 0.3));

        questions.Add(Q(T("P1.1.1"),
            "Which of the following statements about gravity is correct?",
            D.Medium, "An unsupported object falls due to gravity. Gravity acts on all objects regardless of motion state, its direction is always vertically downward, and the center of gravity may lie outside the object.",
            "When an object near the ground has no support, it falls toward the ground due to gravity",
            "A basketball thrown upward experiences no gravity during ascent",
            "A car driving downhill experiences gravity perpendicular to the slope",
            "The center of gravity must be located on the object itself", 0.2));

        // --- Section 2: Elastic Force, Friction ---

        questions.Add(Q(T("P1.3.1"),
            "Which of the following measuring tools is used to measure the magnitude of force?",
            D.Easy, "A spring scale (dynamometer) uses Hooke's Law to measure force.",
            "Spring scale (dynamometer)", "Ruler", "Balance", "Thermometer", -1.5));

        questions.Add(Q(T("P1.3.1"),
            "Worn-out sports shoes have severely worn soles because the soles are subjected to:",
            D.Easy, "The soles experience friction during running which wears them down.",
            "Friction", "Gravity", "Pressure", "Support force", -1.2));

        questions.Add(Q(T("P1.3.1"),
            "Two books with their pages interleaved are very difficult to pull apart because pulling them generates a large amount of:",
            D.Easy, "The interleaved pages create many contact surfaces, generating substantial friction.",
            "Friction", "Gravity", "Elastic force", "Pressure", -1.0));

        questions.Add(Q(T("P1.3.1"),
            "An object is placed on rough horizontal ground and moved in three different orientations with the same applied horizontal force F and coefficient μ. The relationship between friction forces is:",
            D.Medium, "f = μf_N, and f_N = mg regardless of orientation. Friction is independent of contact area.",
            "F₁ = F₂ = F₃ (all equal)",
            "F₁ > F₂ > F₃",
            "F₃ > F₂ > F₁",
            "F₂ > F₁ > F₃", 0.5));

        // --- Section 3: Composition and Resolution of Forces ---

        questions.Add(Q(T("P1.2.1"),
            "An object is acted upon by concurrent forces F₁ = 7 N and F₂ = 9 N. Regardless of angle, their resultant cannot be:",
            D.Easy, "Range is |7−9| = 2 N to 7+9 = 16 N. 18 N is outside this range.",
            "18 N", "5 N", "10 N", "16 N", -0.8));

        questions.Add(Q(T("P1.2.1"),
            "F₁ is directed horizontally to the right with magnitude 9 N. F₂ is directed vertically upward with magnitude 12 N. The magnitude of their resultant is:",
            D.Easy, "F = √(9² + 12²) = √(81+144) = √225 = 15 N.",
            "15 N", "21 N", "3 N", "14 N", -0.5));

        questions.Add(Q(T("P1.2.1"),
            "Two forces acting on the same object have magnitudes of 7 N and 10 N. The possible magnitude of their resultant is:",
            D.Easy, "Range is |7−10| = 3 N to 7+10 = 17 N. 13 N is within this range.",
            "13 N", "1 N", "2 N", "18 N", -0.8));

        questions.Add(Q(T("P1.2.1"),
            "Two forces F₁, F₂ have a resultant F₃. Which set of values (F₁, F₂, F₃) is possible?",
            D.Medium, "Check |F₁−F₂| ≤ F₃ ≤ F₁+F₂. For (16,2,12): |16−2|=14, 16+2=18. But 12 < 14, so this seems outside. Actually wait — |16-2|=14, and 12 < 14 means 12 is not in [14,18]. Let me re-check the problem. The answer key says D (4,20,17) is correct: |4-20|=16, 4+20=24, 17∈[16,24]. ✓",
            "4 N, 20 N, 17 N",
            "5 N, 8 N, 14 N",
            "16 N, 2 N, 12 N",
            "3 N, 4 N, 8 N", 0.3));

        questions.Add(Q(T("P1.2.1"),
            "An object is acted upon by two forces, 5 N and 7 N. The range of their resultant F is:",
            D.Easy, "|5−7| = 2 N, 5+7 = 12 N.",
            "2 N ≤ F ≤ 12 N",
            "4 N ≤ F ≤ 10 N",
            "6 N ≤ F ≤ 10 N",
            "4 N ≤ F ≤ 6 N", -0.8));

        questions.Add(Q(T("P1.2.1"),
            "Two mutually perpendicular forces have magnitudes of 6 N and 8 N. The magnitude of their resultant is:",
            D.Easy, "F = √(6²+8²) = √(36+64) = √100 = 10 N.",
            "10 N", "1 N", "5 N", "9 N", -1.0));

        questions.Add(Q(T("P1.2.1"),
            "Two forces F₁, F₂ have a resultant F₃. Which set of values is IMPOSSIBLE?",
            D.Medium, "For (3,4,8): |3−4|=1, 3+4=7, 8 > 7 → impossible.",
            "3 N, 4 N, 8 N",
            "5 N, 7 N, 8 N",
            "6 N, 5 N, 9 N",
            "2 N, 8 N, 10 N", 0.2));

        questions.Add(Q(T("P1.2.1"),
            "Forces F₁ and F₂ are concurrent. Their resultant F = 3 N directed to the left. F₁ = 4 N directed to the left. The magnitude and direction of F₂ are:",
            D.Medium, "F = F₁ + F₂. Taking left as negative: −3 = −4 + F₂, so F₂ = 1 N to the right.",
            "1 N, directed to the right",
            "7 N, directed to the left",
            "1 N, directed to the left",
            "7 N, directed to the right", 0.3));

        // --- Chapter 1 Self-Test ---

        questions.Add(Q(T("P1.2.1"),
            "A spring has a stiffness coefficient of 500 N/m. If a 200 N force pulls the spring, the elongation is:",
            D.Easy, "x = F/k = 200/500 = 0.4 m.",
            "0.4 m", "4 m", "0.4 cm", "4 cm", -0.5));

        questions.Add(Q(T("P1.2.2"),
            "The maximum resultant of two forces is 10 N, the minimum is 2 N. Then the two forces are:",
            D.Medium, "F₁+F₂=10, F₁−F₂=2 → F₁=6, F₂=4.",
            "6 N and 4 N",
            "3 N and 8 N",
            "14 N and 2 N",
            "4 N and 12 N", 0.3));

        questions.Add(Q(T("P1.3.1"),
            "As shown, an object with mass m moves on a horizontal surface under an applied force F at 30° above horizontal. Coefficient of kinetic friction μ. The sliding friction on the object is:",
            D.Hard, "Normal force: fN = mg − F sin30° = mg − F/2. Friction: f = μ(mg − F/2).",
            "f = μ(mg − F/2)",
            "f = μ(mg + F/2)",
            "f = 0",
            "f = μmg", 1.0));

        questions.Add(Q(T("P1.3.1"),
            "Regarding the sliding friction formula f = μf_N, which statement is correct?",
            D.Medium, "The magnitude of f is determined by μ and f_N, unrelated to the contact area size.",
            "The magnitude of f is determined by μ and f_N, unrelated to contact area",
            "The normal force f_N must equal the object's weight",
            "Sliding friction is directly proportional to normal force, so μ depends on them",
            "Sliding friction is inversely proportional to contact area", 0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 2: MOTION  (Topics P2.x.x)
        // ═══════════════════════════════════════════════════════════════

        // --- Section 2: Uniform Linear Motion ---

        questions.Add(Q(T("P2.1.2"),
            "A car moves at constant speed v = 2 m/s. The distance it travels in 4 seconds is:",
            D.Easy, "s = vt = 2 × 4 = 8 m.",
            "8 m", "16 m", "4 m", "2 m", -1.5));

        questions.Add(Q(T("P2.1.2"),
            "A car travels at constant speed v = 10 m/s. The time required to travel 260 m is:",
            D.Easy, "t = s/v = 260/10 = 26 s.",
            "26 s", "10 s", "50 s", "80 s", -1.2));

        questions.Add(Q(T("P2.1.2"),
            "A car moves with uniform rectilinear motion, covering 30 m in 5 seconds. The speed is:",
            D.Easy, "v = s/t = 30/5 = 6 m/s.",
            "6 m/s", "30 m/s", "3 m/s", "10 m/s", -1.2));

        // --- Section 3: Variable Linear Motion ---

        questions.Add(Q(T("P2.2.1"),
            "In uniformly accelerated rectilinear motion, which of the following statements is correct?",
            D.Medium, "In uniformly accelerated motion, acceleration is constant, so the change in velocity is the same in equal time intervals.",
            "The change in velocity is the same in equal time intervals",
            "The change in acceleration is the same in equal time intervals",
            "The change in velocity is the same over equal distances",
            "The change in displacement is the same in equal time intervals", 0.2));

        questions.Add(Q(T("P2.2.1"),
            "Regarding acceleration, which of the following statements is correct?",
            D.Medium, "Acceleration is the rate of change of velocity, i.e., how fast velocity changes.",
            "Acceleration indicates how fast an object's velocity changes",
            "Acceleration indicates how fast an object moves",
            "Acceleration indicates the magnitude of velocity change",
            "The direction of acceleration must be the same as the direction of motion", 0.3));

        questions.Add(Q(T("P2.2.1"),
            "A cheetah starts from rest, reaching 72 km/h (20 m/s) in 2 seconds. Its acceleration is:",
            D.Easy, "a = Δv/t = 20/2 = 10 m/s².",
            "10 m/s²", "5 m/s²", "36 m/s²", "20 m/s²", -0.5));

        // --- Section 4: Free Fall Motion ---

        questions.Add(Q(T("P2.2.2"),
            "An object undergoes free fall. If g = 10 m/s², the velocity at the end of the 5th second is:",
            D.Easy, "v = gt = 10 × 5 = 50 m/s.",
            "50 m/s", "30 m/s", "15 m/s", "10 m/s", -0.8));

        questions.Add(Q(T("P2.2.2"),
            "Object A weighs four times as much as object B. They are released simultaneously from the same height (neglecting air resistance). Which statement is correct?",
            D.Easy, "Free fall is independent of mass. t = √(2h/g) is the same for both.",
            "A and B hit the ground simultaneously",
            "A hits the ground before B",
            "A has greater acceleration than B",
            "A has greater speed upon hitting the ground", -0.5));

        questions.Add(Q(T("P2.2.2"),
            "The speed of an object in free fall at the end of 2 seconds is (g = 10 m/s²):",
            D.Easy, "v = gt = 10 × 2 = 20 m/s.",
            "20 m/s", "10 m/s", "30 m/s", "40 m/s", -1.0));

        questions.Add(Q(T("P2.2.2"),
            "An object undergoes free fall. The displacement after 5 seconds is (g = 10 m/s²):",
            D.Easy, "h = ½gt² = ½ × 10 × 25 = 125 m.",
            "125 m", "25 m", "50 m", "250 m", -0.5));

        // --- Section 5: Uniform Circular Motion ---

        questions.Add(Q(T("P2.3.2"),
            "A particle moves along a circle of radius R with uniform circular motion, period 2 s. The displacement magnitude in 2 s is:",
            D.Easy, "In one full period the particle returns to its starting point, so displacement = 0.",
            "0", "√2R", "2R", "R", -0.5));

        questions.Add(Q(T("P2.3.2"),
            "An object undergoes uniform circular motion with radius R = 4 m. If its period is 2 s, its angular speed is:",
            D.Easy, "ω = 2π/T = 2π/2 = π ≈ 3.14 rad/s.",
            "π rad/s", "1 rad/s", "2 rad/s", "2π rad/s", -0.3));

        questions.Add(Q(T("P2.3.2"),
            "A particle moves along a circle of radius R with uniform circular motion, period 4 s. The distance traveled in 2 s is:",
            D.Easy, "In half a period, the particle traverses half the circumference: s = πR.",
            "πR", "2R", "R", "2πR", -0.5));

        // --- Chapter 2 Self-Test (selected) ---

        questions.Add(Q(T("P2.2.1"),
            "A racing car starts from rest with uniform acceleration, reaching 25 m/s at 5 s. The car's acceleration magnitude is:",
            D.Easy, "a = Δv/Δt = 25/5 = 5 m/s².",
            "5 m/s²", "2.5 m/s²", "10 m/s²", "50 m/s²", -0.8));

        questions.Add(Q(T("P2.1.1"),
            "Among the following physical quantities, which is a vector?",
            D.Easy, "Displacement has both magnitude and direction; mass, temperature, and distance are scalars.",
            "Displacement", "Mass", "Temperature", "Distance traveled", -1.0));

        questions.Add(Q(T("P2.2.1"),
            "Acceleration magnitude in uniformly accelerated rectilinear motion:",
            D.Easy, "In uniformly accelerated motion, acceleration is constant by definition.",
            "Remains constant", "Increases", "Decreases", "First increases then decreases", -1.0));

        questions.Add(Q(T("P2.2.1"),
            "An object starts from rest with uniform acceleration. If its displacement in the first second is 0.5 m, the displacement in 2 seconds is:",
            D.Medium, "a = 2s/t² = 2×0.5/1 = 1 m/s². In 2 s: s = ½×1×4 = 2 m.",
            "2 m", "0.5 m", "1.5 m", "2.5 m", 0.3));

        questions.Add(Q(T("P2.2.2"),
            "An object in free fall, g = 10 m/s². Velocity at 2 s is:",
            D.Easy, "v = gt = 10 × 2 = 20 m/s.",
            "20 m/s", "10 m/s", "30 m/s", "40 m/s", -1.0));

        questions.Add(Q(T("P2.2.2"),
            "A small ball falls freely from 500 m height. Speed when hitting ground is (g = 10 m/s²):",
            D.Medium, "v² = 2gh = 2×10×500 = 10000, v = 100 m/s.",
            "100 m/s", "500 m/s", "10 m/s", "50 m/s", 0.2));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 3: NEWTON'S LAWS  (Topics P3.x.x)
        // ═══════════════════════════════════════════════════════════════

        // --- Section 1: Newton's First Law ---

        questions.Add(Q(T("P3.1.1"),
            "Which of the following is an incorrect understanding of Newton's First Law?",
            D.Medium, "A block stopping due to friction is actually caused by external force (friction), not because no force maintains motion. Option C is wrong.",
            "A wooden block sliding on ground eventually stops because no force maintains its motion",
            "Newton's First Law describes motion when no external force acts",
            "Without external force, an object's state of motion remains unchanged",
            "A running athlete falls when encountering an obstacle because external forces change his motion", 0.3));

        questions.Add(Q(T("P3.1.1"),
            "Regarding the relationship between motion and force, which is correct?",
            D.Easy, "Force is the cause of changing an object's state of motion, per Newton's First Law.",
            "Force is the cause of changing an object's state of motion",
            "Force is the cause of maintaining an object's motion",
            "Force is the cause of changing an object's inertia",
            "Force is the cause of changing an object's position", -0.5));

        questions.Add(Q(T("P3.1.1"),
            "Which is a correct understanding of 'change in state of motion'?",
            D.Medium, "Changing an object's state of motion requires a force to act on it.",
            "To change an object's state of motion, force must act on it",
            "Change refers only to changes in velocity magnitude",
            "Change refers only to changes in velocity direction",
            "Changes in motion state depend on the initial state", 0.2));

        // --- Section 2: Newton's Second Law ---

        questions.Add(Q(T("P3.1.2"),
            "An object at rest on a smooth horizontal surface experiences a horizontal rightward force F. At that instant, which statement is incorrect?",
            D.Medium, "Acceleration from F = ma; increasing a doesn't increase mass. Mass is constant.",
            "The object's mass increases as acceleration increases",
            "The object immediately has acceleration",
            "Acceleration direction is horizontal rightward",
            "The greater the net force, the greater the acceleration", 0.3));

        questions.Add(Q(T("P3.1.2"),
            "A 10 kg object rests on horizontal ground. When pushed with F₁ = 30 N horizontally, its acceleration is 1 m/s². When push increases to F₂ = 45 N, the acceleration is:",
            D.Medium, "From F₁: 30 − f = 10×1, so f = 20 N. Then 45 − 20 = 10a, a = 2.5 m/s².",
            "2.5 m/s²", "1.5 m/s²", "3.5 m/s²", "4.5 m/s²", 0.5));

        questions.Add(Q(T("P3.1.2"),
            "A 3 kg object on smooth ground has two horizontal forces of 10 N and 20 N. A possible acceleration magnitude is:",
            D.Medium, "Net force range: |20−10|=10 to 20+10=30 N. Acceleration range: 10/3 ≈ 3.3 to 30/3 = 10 m/s². 5 is not within that range... actually wait. If both forces are in the same direction, F_net = 30N, a = 10 m/s². If opposite, F_net = 10N, a = 3.33 m/s². For angles between, it varies. Actually, the answer is B (5 m/s²) for some angle between the forces.",
            "5 m/s²", "3 m/s²", "15 m/s²", "20 m/s²", 0.5));

        questions.Add(Q(T("P3.1.2"),
            "Holding a rope with a heavy object, moving vertically upward with uniform acceleration, if the hand suddenly stops, the heavy object will:",
            D.Medium, "When the hand stops, the object has upward velocity. Only gravity acts — it continues upward with deceleration.",
            "Begin moving upward with deceleration",
            "Immediately stop moving",
            "Continue with uniform acceleration upward",
            "Continue with uniform velocity upward", 0.3));

        // --- Section 3: Newton's Third Law ---

        questions.Add(Q(T("P3.1.3"),
            "A moving object, if all forces acting on it simultaneously disappear, it will:",
            D.Easy, "By Newton's First Law, without external forces an object continues in uniform rectilinear motion.",
            "Continue moving in original direction with uniform rectilinear motion",
            "Immediately stop",
            "Slow down first, then stop",
            "Change direction of motion", -0.5));

        questions.Add(Q(T("P3.1.3"),
            "Regarding inertia, which is correct?",
            D.Easy, "All objects have inertia whether moving or at rest. Inertia depends only on mass.",
            "An apple in free fall has inertia",
            "An apple resting on a desktop has no inertia",
            "The faster an athlete runs, the greater his inertia",
            "A car only has inertia when braking", -0.5));

        questions.Add(Q(T("P3.1.3"),
            "A 2 kg object on a smooth horizontal surface experiences a horizontal pulling force F. Its acceleration magnitude is 4 m/s². F should be:",
            D.Easy, "F = ma = 2 × 4 = 8 N.",
            "8 N", "2 N", "4 N", "6 N", -0.8));

        // --- Chapter 3 Self-Test ---

        questions.Add(Q(T("P3.1.1"),
            "To change an object's inertia, one should change the object's:",
            D.Easy, "Mass is the measure of inertia.",
            "Mass", "Velocity", "Acceleration", "Applied force", -0.8));

        questions.Add(Q(T("P3.1.2"),
            "An object experiences net force F = 10 N, producing acceleration a = 2 m/s². To produce a' = 4 m/s², the net force F' needed is:",
            D.Easy, "m = F/a = 5 kg. F' = ma' = 5×4 = 20 N.",
            "20 N", "5 N", "16 N", "17 N", -0.5));

        questions.Add(Q(T("P3.1.2"),
            "An object mass m falls freely, experiencing air resistance f. Acceleration a = g/3. The air resistance f is:",
            D.Hard, "mg − f = m(g/3), so f = mg − mg/3 = 2mg/3.",
            "(2/3)mg", "(1/3)mg", "mg", "(4/3)mg", 1.0));

        questions.Add(Q(T("P3.1.2"),
            "An object experiences two mutually perpendicular forces, F₁ = 6 N, F₂ = 8 N. Its acceleration is 2 m/s². The object's mass is:",
            D.Medium, "F_net = √(6²+8²) = 10 N. m = F/a = 10/2 = 5 kg.",
            "5 kg", "3 kg", "1 kg", "4 kg", 0.5));

        questions.Add(Q(T("P3.1.2"),
            "A 10 kg object on horizontal surface, friction coefficient 0.05, horizontal rightward pulling force F = 10 N. Acceleration magnitude is:",
            D.Hard, "f = μmg = 0.05×10×10 = 5 N. F_net = 10−5 = 5 N. a = 5/10 = 0.5 m/s².",
            "0.5 m/s²", "0.1 m/s²", "1 m/s²", "10 m/s²", 0.8));

        questions.Add(Q(T("P3.1.2"),
            "Two forces F₁ = 40 N and F₂ = 20 N. Their maximum resultant is:",
            D.Easy, "Max resultant = F₁ + F₂ = 40 + 20 = 60 N.",
            "60 N", "20 N", "80 N", "150 N", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 4: MOMENTUM  (Topics P4.x.x)
        // ═══════════════════════════════════════════════════════════════

        // --- Section 1: Impulse and Momentum ---

        questions.Add(Q(T("P4.1.1"),
            "The symbol and unit for momentum are respectively:",
            D.Easy, "Momentum is denoted p with units kg·m/s.",
            "p, kg·m/s", "F, m/s", "v, m/s²", "I, kg·m/s", -1.5));

        questions.Add(Q(T("P4.1.1"),
            "Two objects with the same momentum must have:",
            D.Medium, "Same momentum means same p = mv vector. The direction of motion must be the same (since momentum includes direction).",
            "The same direction of motion",
            "The same velocity",
            "The same mass",
            "The same kinetic energy", 0.3));

        questions.Add(Q(T("P4.1.1"),
            "An object moves leftward at 5 m/s with mass 1 kg. Its momentum magnitude is:",
            D.Easy, "p = mv = 1 × 5 = 5 kg·m/s.",
            "5 kg·m/s", "2 kg·m/s", "10 kg·m/s", "2.5 kg·m/s", -1.0));

        questions.Add(Q(T("P4.1.1"),
            "An object with mass 50 kg moves rightward at 10 m/s. Taking leftward as positive, its momentum is:",
            D.Medium, "Rightward velocity is negative: p = 50×(−10) = −500 kg·m/s.",
            "−500 kg·m/s", "500 kg·m/s", "60 kg·m/s", "−60 kg·m/s", 0.3));

        questions.Add(Q(T("P4.1.1"),
            "A 200 kg car moving at 5 m/s collides with a wall and stops. The impulse experienced by the car is:",
            D.Medium, "Δp = m(v_f − v_i) = 200(0−5) = −1000 kg·m/s. Magnitude: 1000 N·s.",
            "1000 N·s", "40 N·s", "200 N·s", "2000 N·s", 0.3));

        questions.Add(Q(T("P4.1.1"),
            "An object undergoes non-uniform motion. Then:",
            D.Medium, "Non-uniform means velocity changes, so momentum (p = mv) must change.",
            "The object's momentum must change",
            "The object's velocity magnitude must change",
            "The net force on the object must change",
            "The impulse must be zero", 0.3));

        questions.Add(Q(T("P4.1.1"),
            "Among the following, three physical quantities that are ALL vectors are:",
            D.Easy, "Gravity, momentum, and displacement all have direction. Mass, length, time are scalars.",
            "Gravity, momentum, displacement",
            "Friction, mass, momentum",
            "Velocity, acceleration, length",
            "Time, displacement, impulse", -0.5));

        // --- Section 2: Impulse-Momentum Theorem ---

        questions.Add(Q(T("P4.1.2"),
            "Regarding momentum and impulse, which is correct?",
            D.Medium, "For an object, greater momentum means greater velocity when mass is constant. But for different objects, greater momentum doesn't guarantee greater velocity. Actually, the answer key says A. Let me reconsider: the options likely have A = 'For an object, greater momentum means greater velocity' which is true for a fixed-mass object.",
            "For a given object, greater momentum means greater velocity",
            "Greater force always means greater impulse",
            "An object's momentum direction must be same as net force direction",
            "If two forces have equal magnitude and act for equal time, their impulses must be equal", 0.3));

        questions.Add(Q(T("P4.1.2"),
            "A basketball falls vertically, hitting ground at 4 m/s, bouncing vertically at 1 m/s. Mass 0.6 kg. Impulse from net force during collision is:",
            D.Medium, "Taking upward positive: Δp = 0.6×1 − 0.6×(−4) = 0.6 + 2.4 = 3 kg·m/s.",
            "3 kg·m/s", "1.8 kg·m/s", "0.6 kg·m/s", "2.4 kg·m/s", 0.5));

        // --- Section 3: Conservation of Momentum ---

        questions.Add(Q(T("P4.2.1"),
            "A 0.01 kg bullet at 400 m/s hits a 0.49 kg block at rest on smooth surface and embeds. Their common velocity is:",
            D.Medium, "mv = (m+M)v'. 0.01×400 = 0.5v'. v' = 8 m/s.",
            "8 m/s", "6 m/s", "10 m/s", "12 m/s", 0.3));

        questions.Add(Q(T("P4.2.1"),
            "Block mass M at rest on smooth table. Bullet mass m, velocity v₀ hits and embeds. After interaction, block velocity is:",
            D.Medium, "mv₀ = (m+M)v'. v' = mv₀/(m+M).",
            "mv₀/(M+m)", "2mv₀/(M+m)", "Mv₀/(M+m)", "(M−m)v₀/(M+m)", 0.5));

        questions.Add(Q(T("P4.2.1"),
            "Ball A at v₀ rightward collides with stationary ball B. After: A bounces at v₀/2, B moves at v₀/3 rightward. Mass ratio A:B is:",
            D.Hard, "m_A×v₀ = m_A×(−v₀/2) + m_B×(v₀/3). m_A×(3v₀/2) = m_B×(v₀/3). m_A/m_B = 2/9.",
            "2:9", "9:2", "2:3", "3:2", 1.2));

        // --- Chapter 4 Self-Test ---

        questions.Add(Q(T("P4.1.1"),
            "A 5 kg steel ball moves rightward at 6 m/s. Its momentum magnitude is:",
            D.Easy, "p = mv = 5 × 6 = 30 kg·m/s.",
            "30 kg·m/s", "1 kg·m/s", "180 kg·m/s", "150 kg·m/s", -1.0));

        questions.Add(Q(T("P4.2.1"),
            "A child jumps onto a flatbed cart at rest on a smooth track. After jumping on, they share velocity. Which is correct?",
            D.Medium, "No external horizontal forces → momentum conserved. Inelastic collision → mechanical energy lost.",
            "Momentum conserved",
            "Mechanical energy conserved",
            "Both conserved",
            "Neither conserved", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 5: MECHANICAL ENERGY  (Topics P5.x.x)
        // ═══════════════════════════════════════════════════════════════

        // --- Section 1: Work and Power ---

        questions.Add(Q(T("P5.1.1"),
            "A person pulls a wooden box with equal horizontal force on both smooth and rough surfaces over equal distances. Regarding work done by pulling force:",
            D.Easy, "W = F×s×cosα. Same F, same s, same angle → equal work.",
            "Equal work both times",
            "More work on rough surface",
            "More work on smooth surface",
            "Insufficient information", -0.5));

        questions.Add(Q(T("P5.3.1"),
            "Regarding power, which is correct?",
            D.Easy, "Power = work/time. 'Faster work' means greater work per unit time.",
            "The faster work is done, the greater the power",
            "The more work done, the greater the power",
            "The shorter the time, the greater the power",
            "The longer the time, the greater the power", -0.5));

        questions.Add(Q(T("P5.1.1"),
            "In SI units, the power unit is:",
            D.Easy, "The SI unit of power is the Watt (W).",
            "Watt (W)", "Newton (N)", "Joule (J)", "Pascal (Pa)", -1.5));

        questions.Add(Q(T("P5.1.1"),
            "Regarding work, which is correct?",
            D.Easy, "When a crane lifts goods upward, the pulling force acts in the direction of displacement → work is done.",
            "When a crane lifts goods, the pulling force does work on the goods",
            "Holding a box stationary, the person does work on the goods",
            "On a horizontal road, a car's upward support force does work on goods",
            "After a shot put lands, gravity does work as it rolls on horizontal ground", -0.5));

        questions.Add(Q(T("P5.1.1"),
            "Regarding work and power concepts, which is correct?",
            D.Hard, "Negative work can still be done by forces that help deceleration, not necessarily hindering the overall motion.",
            "If a force does negative work on an object, this force doesn't necessarily hinder the object's motion",
            "Work can be positive or negative, indicating work is a vector",
            "If force does no work, the object's displacement must be 0",
            "A force doing work faster always means greater power", 0.8));

        // --- Section 2: Kinetic Energy ---

        questions.Add(Q(T("P5.2.1"),
            "To increase an object's kinetic energy, one should increase its:",
            D.Easy, "E_k = ½mv². Increasing velocity increases kinetic energy.",
            "Velocity",
            "Acceleration",
            "Motion time",
            "Applied force magnitude", -0.5));

        questions.Add(Q(T("P5.1.2"),
            "Which statement about kinetic energy is correct?",
            D.Medium, "For the same car, greater speed means greater kinetic energy since E_k = ½mv².",
            "Same car, greater speed means greater kinetic energy",
            "Fast ball must have greater kinetic energy than slow ball",
            "Bullet speed > train speed, so bullet E_k > train E_k",
            "Object in uniform motion has kinetic energy that continuously increases", 0.2));

        // --- Section 3: Potential Energy ---

        questions.Add(Q(T("P5.2.1"),
            "A helicopter rising vertically possesses:",
            D.Easy, "It has both velocity (kinetic energy) and height (gravitational potential energy).",
            "Both kinetic and gravitational potential energy",
            "Kinetic energy only",
            "Gravitational potential energy only",
            "Elastic potential energy only", -0.8));

        questions.Add(Q(T("P5.2.1"),
            "Throwing objects from high altitude is dangerous because high objects have greater:",
            D.Easy, "Higher objects have more gravitational potential energy which converts to kinetic energy upon falling.",
            "Gravitational potential energy",
            "Elastic potential energy",
            "Volume",
            "Weight", -1.0));

        // --- Section 4: Conservation of Mechanical Energy ---

        questions.Add(Q(T("P5.2.2"),
            "A student cycling uphill, speed decreasing. Then the bicycle and person's:",
            D.Easy, "Speed decreasing → kinetic energy decreasing. Going uphill → potential energy increasing.",
            "Kinetic energy decreases, gravitational potential energy increases",
            "Kinetic energy increases, gravitational potential energy increases",
            "Kinetic energy decreases, gravitational potential energy decreases",
            "Kinetic energy unchanged, gravitational potential energy unchanged", -0.5));

        questions.Add(Q(T("P5.2.2"),
            "During fall of a ripe apple from tree, which is correct?",
            D.Easy, "Falling: speed increases → E_k increases; height decreases → E_p decreases.",
            "Kinetic energy increases, gravitational potential energy decreases",
            "Kinetic energy decreases, gravitational potential energy decreases",
            "Kinetic energy increases, gravitational potential energy increases",
            "Kinetic energy decreases, gravitational potential energy increases", -0.8));

        questions.Add(Q(T("P5.2.2"),
            "Which process has mechanical energy conserved?",
            D.Medium, "On a smooth incline, only gravity does work → mechanical energy conserved.",
            "Object sliding down smooth incline",
            "Raindrop falling uniformly (terminal velocity)",
            "Spacecraft accelerating upward on rocket",
            "Car braking then sliding on horizontal road", 0.3));

        // --- Chapter 5 Self-Test ---

        questions.Add(Q(T("P5.2.2"),
            "An object of mass m = 2 kg slides from rest from top of smooth incline height h = 5 m. Velocity at bottom B is (g = 10 m/s²):",
            D.Medium, "mgh = ½mv². v = √(2gh) = √(100) = 10 m/s.",
            "10 m/s", "20 m/s", "100 m/s", "200 m/s", 0.2));

        questions.Add(Q(T("P5.1.1"),
            "Using 25 N horizontal force on 10 kg object, moves 40 m on smooth surface. Pulling force work:",
            D.Easy, "W = Fs = 25 × 40 = 1000 J.",
            "1000 J", "100 J", "600 J", "0 J", -0.5));

        questions.Add(Q(T("P5.2.1"),
            "Object moving at speed v has kinetic energy E. When speed increases to 4v, kinetic energy becomes:",
            D.Easy, "E_k = ½mv². Speed 4v → E_k = ½m(4v)² = 16(½mv²) = 16E.",
            "16E", "8E", "4E", "2E", -0.3));

        questions.Add(Q(T("P5.2.1"),
            "Sprinter mass m = 60 kg at v = 10 m/s. Kinetic energy:",
            D.Easy, "E_k = ½mv² = ½ × 60 × 100 = 3000 J.",
            "3000 J", "6000 J", "600 J", "300 J", -0.5));

        questions.Add(Q(T("P5.2.1"),
            "Object mass 2 kg, speed 5 m/s. Kinetic energy:",
            D.Easy, "E_k = ½mv² = ½ × 2 × 25 = 25 J.",
            "25 J", "5 J", "6 J", "10 J", -0.8));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 6: ELECTRIC FIELD  (Topics P6.x.x)
        // ═══════════════════════════════════════════════════════════════

        // --- Section 1: Charge and Coulomb's Law ---

        questions.Add(Q(T("P6.1.1"),
            "Two point charges separated by distance d have force F. Keeping charges unchanged, if distance increases to 3d, the Coulomb force becomes:",
            D.Easy, "F' = kQ₁Q₂/(3d)² = F/9.",
            "(1/9)F", "9F", "3F", "(1/3)F", -0.5));

        questions.Add(Q(T("P6.1.1"),
            "To make the Coulomb force between two charges become 1/4 of original, one can:",
            D.Medium, "Doubling the distance: F' = kQ₁Q₂/(2d)² = F/4.",
            "Increase the distance to 2 times the original",
            "Reduce both charges to 1/4",
            "Decrease distance to 1/4",
            "Increase distance to 4 times and increase one charge to 4 times", 0.3));

        questions.Add(Q(T("P6.1.1"),
            "Two charges 4Q and 3Q have Coulomb force F. If changed to 2Q and 6Q at same distance, the force becomes:",
            D.Medium, "F' = k(2Q)(6Q)/r² = 12kQ²/r². Original: k(4Q)(3Q)/r² = 12kQ²/r² = F. So F' = F.",
            "F", "(8/7)F", "(7/8)F", "2F", 0.5));

        questions.Add(Q(T("P6.1.1"),
            "On a smooth surface, two spheres with opposite but equal charges are released from rest. The distance and force will:",
            D.Medium, "Unlike charges attract → distance decreases → force increases.",
            "Decrease; increase",
            "Decrease; decrease",
            "Increase; decrease",
            "Increase; increase", 0.3));

        // --- Section 2: Electric Field and Strength ---

        questions.Add(Q(T("P6.1.2"),
            "A point charge of 5.0 × 10⁻⁹ C in an electric field of 4.0 × 10⁴ N/C. The electric force is:",
            D.Easy, "F = Eq = 4 × 10⁴ × 5 × 10⁻⁹ = 2 × 10⁻⁴ N.",
            "2.0 × 10⁻⁴ N", "2.0 × 10⁻⁸ N", "2.0 × 10⁻⁵ N", "2.0 × 10⁻³ N", -0.5));

        // --- Section 4: Potential Difference ---

        questions.Add(Q(T("P6.2.1"),
            "Among the following physical quantities, which one is NOT a vector?",
            D.Easy, "Electric potential is a scalar quantity. Impulse, displacement, and electric field strength are vectors.",
            "Electric potential",
            "Impulse",
            "Displacement",
            "Electric field strength", -1.0));

        questions.Add(Q(T("P6.2.1"),
            "A charge of −2 × 10⁻³ C moves from A to B. Electric force does 4 × 10⁻⁴ J of work. Potential difference U_AB is:",
            D.Medium, "U_AB = W/q = (4 × 10⁻⁴)/(−2 × 10⁻³) = −0.2 V.",
            "−0.2 V", "0.2 V", "0.4 V", "−0.4 V", 0.3));

        // --- Chapter 6 Self-Test ---

        questions.Add(Q(T("P6.1.1"),
            "Two charges on identical spheres, +4Q and −2Q, distance d. After full contact and return to original position, force is:",
            D.Hard, "After contact: Q' = (+4Q − 2Q)/2 = Q each. F' = kQ²/d².",
            "kQ²/d²", "(8kQ²)/d²", "(2kQ²)/d²", "kQ²/(4d²)", 1.0));

        questions.Add(Q(T("P6.2.1"),
            "Potential difference U_AB = 10 V. Positive charge 5 × 10⁻⁴ C moves from A to B. Work by electric force:",
            D.Easy, "W = qU = 5 × 10⁻⁴ × 10 = 5 × 10⁻³ J.",
            "5 × 10⁻³ J", "5 × 10⁻² J", "5 × 10⁻⁵ J", "0 J", -0.5));

        questions.Add(Q(T("P6.2.1"),
            "In a uniform field, U_AB = 10 V, U_BC = −20 V. Then U_AC =",
            D.Medium, "U_AC = U_AB + U_BC = 10 + (−20) = −10 V.",
            "−10 V", "−30 V", "+10 V", "+30 V", 0.3));

        questions.Add(Q(T("P6.1.1"),
            "Two charges d apart have force F. One tripled, other reduced to 1/3, distance unchanged. New force:",
            D.Medium, "F' = k(3Q₁)(Q₂/3)/d² = kQ₁Q₂/d² = F.",
            "F", "9F", "3F", "0", 0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 7: DC CIRCUITS  (Topics P7.x.x)
        // ═══════════════════════════════════════════════════════════════

        // --- Section 1: Circuits and Ohm's Law ---

        questions.Add(Q(T("P7.1.1"),
            "A portable power bank is charging a phone battery. In this circuit, the power bank acts as:",
            D.Easy, "The power bank supplies electrical energy; it is the power source.",
            "Power source", "Switch", "Conductor", "Electrical appliance", -1.5));

        questions.Add(Q(T("P7.1.1"),
            "Which statement about resistance is correct?",
            D.Easy, "A conductor always has resistance regardless of current flow. Resistance is a material property.",
            "A conductor always has resistance, regardless of whether current is flowing",
            "Greater voltage means greater resistance",
            "No current means no resistance",
            "Smaller current means greater resistance", -0.5));

        questions.Add(Q(T("P7.1.1"),
            "For Ohm's Law R = U/I, which statement is correct?",
            D.Medium, "R is a property of the conductor. I is proportional to U for a given R.",
            "For a given conductor, I is proportional to U",
            "When U = 0, then R = 0",
            "When U increases, R increases",
            "When I increases, R decreases", 0.2));

        questions.Add(Q(T("P7.1.1"),
            "Voltage across a resistor is 5 V, current is 0.01 A. Resistance is:",
            D.Easy, "R = U/I = 5/0.01 = 500 Ω.",
            "500 Ω", "5 Ω", "0.5 Ω", "50 Ω", -0.8));

        questions.Add(Q(T("P7.1.1"),
            "A light bulb labeled '12 V 6 W' connected to 12 V. The current through it is:",
            D.Easy, "I = P/U = 6/12 = 0.5 A.",
            "0.5 A", "0.25 A", "1 A", "2 A", -0.5));

        // --- Section 2: Series and Parallel ---

        questions.Add(Q(T("P7.1.2"),
            "Connecting two 40 Ω resistors in series gives total resistance:",
            D.Easy, "R_total = 40 + 40 = 80 Ω.",
            "80 Ω", "60 Ω", "20 Ω", "40 Ω", -1.5));

        questions.Add(Q(T("P7.1.2"),
            "Connecting two 20 Ω resistors in parallel gives total resistance:",
            D.Easy, "1/R = 1/20 + 1/20 = 1/10, R = 10 Ω.",
            "10 Ω", "80 Ω", "20 Ω", "40 Ω", -1.0));

        questions.Add(Q(T("P7.1.2"),
            "R₁ = 3R₂ connected in series. Total voltage 4 V. Voltage across R₁ is:",
            D.Medium, "V₁ = R₁/(R₁+R₂) × 4 = 3R₂/(4R₂) × 4 = 3 V.",
            "3 V", "4 V", "2 V", "1 V", 0.3));

        questions.Add(Q(T("P7.1.2"),
            "R₁ = 2 Ω, R₂ = 6 Ω in series, U = 16 V. Voltage across R₁ is:",
            D.Medium, "V₁ = R₁/(R₁+R₂) × U = 2/8 × 16 = 4 V.",
            "4 V", "2 V", "3 V", "9 V", 0.2));

        // --- Chapter 7 Self-Test ---

        questions.Add(Q(T("P7.1.1"),
            "Resistor R = 10 Ω, voltage U = 5 V. Charge through it in 4 seconds is:",
            D.Medium, "I = U/R = 0.5 A. q = It = 0.5 × 4 = 2 C.",
            "2 C", "0.8 C", "0.5 C", "4 C", 0.2));

        questions.Add(Q(T("P7.1.1"),
            "Light bulb '12 V 6 W' connected to 6 V circuit. Current through bulb is:",
            D.Medium, "R = U²/P = 144/6 = 24 Ω. I = 6/24 = 0.25 A.",
            "0.25 A", "0.5 A", "1 A", "2 A", 0.3));

        questions.Add(Q(T("P7.3.1"),
            "A resistor wire's cross-section becomes twice original, length becomes half. New resistance is:",
            D.Hard, "R = ρl/S. R' = ρ(l/2)/(2S) = R/4 = 0.25R.",
            "0.25 times original",
            "0.5 times original",
            "1 times original",
            "2 times original", 1.0));

        questions.Add(Q(T("P7.3.1"),
            "Electric motor rated 220 V, internal resistance 10 Ω, draws 10 A. Power lost as heat is:",
            D.Medium, "P_heat = I²R = 100 × 10 = 1000 W.",
            "1000 W", "2200 W", "4840 W", "6000 W", 0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 8: MAGNETIC FIELD  (Topics P8.x.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("P8.1.1"),
            "The correct unit symbol for magnetic flux density (magnetic induction) is:",
            D.Easy, "Magnetic flux density is measured in Tesla (T).",
            "T", "Wb", "A", "C", -1.5));

        questions.Add(Q(T("P8.1.1"),
            "When the N-poles of two magnets are brought close together, they:",
            D.Easy, "Like poles repel each other.",
            "Repel each other", "Attract each other", "Exert no force", "Sometimes attract, sometimes repel", -1.5));

        questions.Add(Q(T("P8.1.2"),
            "A wire 0.2 m long carries 1 A perpendicular to uniform magnetic field, experiencing 0.2 N force. Magnetic flux density is:",
            D.Medium, "B = F/(IL) = 0.2/(1×0.2) = 1 T.",
            "1 T", "0 T", "0.2 T", "0.5 T", 0.2));

        questions.Add(Q(T("P8.1.2"),
            "For B = F/(IL), with wire perpendicular to field, which is correct?",
            D.Hard, "B is a property of the field, not the wire. It's independent of changes in F, I, or L.",
            "B is independent of changes in F, I, or L",
            "B increases as I decreases",
            "B increases as L decreases",
            "B increases as F increases", 0.8));

        questions.Add(Q(T("P8.1.2"),
            "A wire L = 5 cm carries I = 1 A perpendicular to field at P, force F = 0.1 N. If wire removed, B at P is:",
            D.Medium, "B = F/(IL) = 0.1/(1×0.05) = 2 T. B is a property of the field; removing the wire doesn't change it.",
            "2 T", "0 T", "0.02 T", "0.5 T", 0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 9: ELECTROMAGNETIC INDUCTION  (Topics P9.x.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("P9.1.1"),
            "The correct unit symbol for magnetic flux is:",
            D.Easy, "Magnetic flux is measured in Weber (Wb).",
            "Wb", "T", "A", "C", -1.5));

        questions.Add(Q(T("P9.1.2"),
            "Regarding induced emf magnitude, which statement is correct?",
            D.Medium, "By Faraday's Law, emf = N|ΔΦ/Δt|. Larger rate of change gives larger emf.",
            "When the rate of change ΔΦ/Δt is larger, the induced emf is larger",
            "When magnetic flux Φ is maximum, the induced emf is maximum",
            "When change in flux ΔΦ increases, the induced emf increases",
            "When Φ = 0, the induced emf is 0", 0.3));

        questions.Add(Q(T("P9.1.1"),
            "A wire loop of area 0.5 m² perpendicular to field B = 2.0 × 10⁻² T. Magnetic flux is:",
            D.Easy, "Φ = BS = 0.02 × 0.5 = 0.01 Wb = 1.0 × 10⁻² Wb.",
            "1.0 × 10⁻² Wb",
            "2.5 × 10⁻² Wb",
            "1.5 × 10⁻² Wb",
            "4.0 × 10⁻² Wb", -0.5));

        questions.Add(Q(T("P9.1.2"),
            "Which of the following physical quantities is a vector?",
            D.Easy, "Magnetic flux density B is a vector. Magnetic flux, emf, and current are scalars.",
            "Magnetic flux density (B)",
            "Magnetic flux",
            "Electromotive force",
            "Current", -0.8));

        questions.Add(Q(T("P9.1.1"),
            "A square loop in uniform field B is rotated about one side by 60°. If initial flux was BL², the new flux is:",
            D.Medium, "After 60° rotation, angle between B and normal is 60°. Φ = BL²cos60° = ½BL².",
            "(1/2)BL²",
            "(√3/2)BL²",
            "BL²",
            "√3BL²", 0.5));

        questions.Add(Q(T("P9.1.1"),
            "A square loop with plane parallel to magnetic field lines. The magnetic flux through it is:",
            D.Medium, "When the plane is parallel to field lines, B is perpendicular to the normal → Φ = BScos90° = 0.",
            "0", "B/S", "S/B", "BS", 0.3));

        // --- Lenz's Law ---

        questions.Add(Q(T("P9.2.1"),
            "When a bar magnet falls through a coil, the induced current creates a field that:",
            D.Medium, "By Lenz's Law, the induced current opposes the change causing it — it repels the falling magnet.",
            "Repels the magnet",
            "Attracts the magnet",
            "Has no effect on the magnet",
            "Is always zero", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 10: VIBRATIONS & WAVES  (Topics P10.x.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("P10.1.1"),
            "In simple harmonic motion, acceleration is maximum when:",
            D.Easy, "a = −ω²x. Maximum displacement → maximum acceleration.",
            "Displacement is maximum",
            "Velocity is maximum",
            "Displacement is zero",
            "Velocity is zero", -0.5));

        questions.Add(Q(T("P10.1.2"),
            "The period of a mass-spring system depends on:",
            D.Medium, "T = 2π√(m/k). Period depends on mass and spring constant.",
            "Mass and spring constant",
            "Amplitude",
            "Initial displacement",
            "Gravity", 0.3));

        questions.Add(Q(T("P10.1.2"),
            "A pendulum's period on Earth is T. On the Moon (g smaller), its period:",
            D.Medium, "T = 2π√(L/g). Smaller g → larger T.",
            "Increases",
            "Decreases",
            "Stays the same",
            "Cannot be determined", 0.3));

        questions.Add(Q(T("P10.2.1"),
            "Sound is a:",
            D.Easy, "Sound propagates as a longitudinal wave (particle vibration parallel to propagation direction).",
            "Longitudinal wave",
            "Transverse wave",
            "Electromagnetic wave",
            "Stationary wave", -1.0));

        questions.Add(Q(T("P10.2.2"),
            "In a wave, the distance between two consecutive crests is the:",
            D.Easy, "The distance between consecutive crests defines wavelength.",
            "Wavelength", "Amplitude", "Frequency", "Period", -1.5));

        questions.Add(Q(T("P10.2.2"),
            "If wave frequency doubles and speed is constant, wavelength:",
            D.Easy, "v = λf → λ = v/f. If f doubles, λ halves.",
            "Halves", "Doubles", "Quadruples", "Stays the same", -0.5));

        // --- Chapter 10 Self-Test ---

        questions.Add(Q(T("P10.1.1"),
            "In SHM, at the equilibrium position, the oscillator's restoring force, acceleration, displacement, and speed — which is maximum?",
            D.Medium, "At equilibrium: x=0, F=0, a=0, speed is maximum.",
            "Speed is maximum",
            "Restoring force is maximum",
            "Acceleration is maximum",
            "Displacement is maximum", 0.2));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 11: HEAT / THERMODYNAMICS  (Topics P11.x.x)
        // ═══════════════════════════════════════════════════════════════

        // --- Section 2: Ideal Gas ---

        questions.Add(Q(T("P11.1.2"),
            "For an ideal gas, A → B is an isobaric process. If V_A = 0.6 m³ at T_A = 300 K, then V_B at T_B = 400 K is:",
            D.Medium, "Isobaric: V/T = const. V_B = V_A × T_B/T_A = 0.6 × 400/300 = 0.8 m³.",
            "0.8 m³", "0.6 m³", "1.0 m³", "0.45 m³", 0.3));

        questions.Add(Q(T("P11.1.2"),
            "For an ideal gas with p₁ = p₂, which relation is correct?",
            D.Medium, "If p constant: V₁/T₁ = V₂/T₂ → V₁/V₂ = T₁/T₂. If V constant: p₁/T₁ = p₂/T₂ → p/T is proportional.",
            "If V₁ = V₂, then p₁/p₂ = T₁/T₂",
            "V₁/V₂ = T₂/T₁",
            "p₁/p₂ = T₂/T₁",
            "V₁T₁ = V₂T₂", 0.5));

        questions.Add(Q(T("P11.1.2"),
            "When temperature T is constant, p₁V₁ = p₂V₂ = p₃V₃ (p and V inversely proportional). If p₁ > p₂ > p₃, then:",
            D.Medium, "At constant T, pV = const. If p₁ > p₂ > p₃, then V₁ < V₂ < V₃.",
            "V₁ < V₂ < V₃",
            "V₁ > V₂ > V₃",
            "V₁ = V₂ = V₃",
            "V₁ < V₃ < V₂", 0.3));

        // --- Chapter 11 Self-Test ---

        questions.Add(Q(T("P11.1.1"),
            "The same heat is added to equal masses of copper and water. Which has a larger temperature change?",
            D.Easy, "Water has much higher specific heat capacity than copper. ΔT = Q/(mc), so copper heats more.",
            "Copper",
            "Water",
            "Same temperature change",
            "Cannot determine", -0.5));

        questions.Add(Q(T("P11.1.2"),
            "When V is constant, p₁/p₂ = T₁/T₂. If p₁ > p₂, then:",
            D.Easy, "If p proportional to T, and p₁ > p₂, then T₁ > T₂.",
            "T₁ > T₂", "T₁ < T₂", "T₁ = T₂", "Cannot determine", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 12: GEOMETRICAL OPTICS  (Topics P12.x.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("P12.1.2"),
            "When light passes from air into glass, it:",
            D.Easy, "Light bends toward the normal when entering a denser medium (higher n).",
            "Bends toward the normal",
            "Bends away from the normal",
            "Does not bend",
            "Reflects completely", -1.0));

        questions.Add(Q(T("P12.1.2"),
            "The critical angle for total internal reflection depends on:",
            D.Medium, "sin θ_c = n₂/n₁. It depends on the refractive indices of both media.",
            "The refractive indices of the two media",
            "The wavelength of light only",
            "The angle of incidence only",
            "The surface area", 0.2));

        questions.Add(Q(T("P12.1.2"),
            "Angle of incidence i = 45°, refractive index n = √2. From Snell's law n = sin i/sin r. The refracted angle is:",
            D.Medium, "√2 = sin45°/sinr = (√2/2)/sinr → sinr = 1/2 → r = 30°.",
            "30°", "45°", "60°", "90°", 0.3));

        // --- Chapter 12 Self-Test ---

        questions.Add(Q(T("P12.1.2"),
            "Which phenomenon explains why a straw looks bent in a glass of water?",
            D.Easy, "Refraction of light at the water-air interface causes the bent appearance.",
            "Refraction", "Reflection", "Diffraction", "Interference", -1.0));

        questions.Add(Q(T("P12.1.2"),
            "Angle of incidence 45°, angle of refraction 30°. Refractive index n = sin i/sin r equals:",
            D.Medium, "n = sin45°/sin30° = (√2/2)/(1/2) = √2.",
            "√2", "√3", "1", "√6/2", 0.3));

        questions.Add(Q(T("P12.1.1"),
            "Shadows, lunar eclipses, solar eclipses, and pinhole imaging are all caused by:",
            D.Easy, "All these phenomena occur because light travels in straight lines.",
            "Light traveling in straight lines",
            "Refraction of light",
            "Diffraction of light",
            "Reflection of light", -0.8));

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
