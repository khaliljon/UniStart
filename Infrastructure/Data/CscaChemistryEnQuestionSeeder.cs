using Microsoft.EntityFrameworkCore;
using UniStart.Domain.Entities;

namespace UniStart.Infrastructure.Data;

/// <summary>
/// Seeds ~200 multiple-choice questions for CSCA Chemistry EN (14 chapters).
/// Each question has exactly 4 answer options, difficulty level, explanation, and IRT parameters.
/// </summary>
public class CscaChemistryEnQuestionSeeder
{
    private readonly UniStartDbContext _context;

    public CscaChemistryEnQuestionSeeder(UniStartDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        var chemSection = await _context.ExamSections
            .FirstOrDefaultAsync(s => s.ExamTypeCode == "CSCA" && s.Name.Contains("Chemistry") && !s.Name.Contains("(CN)"));
        if (chemSection == null) return;

        var existing = await _context.Questions
            .AnyAsync(q => q.Topic.SectionId == chemSection.Id);
        if (existing) return;

        var topics = await _context.Topics
            .Where(t => t.SectionId == chemSection.Id)
            .ToDictionaryAsync(t => t.Name.Split(' ')[0], t => t.Id);

        int T(string prefix) => topics.TryGetValue(prefix, out var id) ? id : topics.Values.First();

        var questions = new List<Question>();

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 1: CHEMICAL FUNDAMENTALS (C1.x.x)
        // ═══════════════════════════════════════════════════════════════

        // --- C1.1.1 Atoms, Molecules, and Ions ---
        questions.Add(Q(T("C1.1.1"), "Which subatomic particle determines the identity of an element?",
            D.Easy, "The number of protons (atomic number) uniquely identifies an element.",
            "Protons", "Neutrons", "Electrons", "Positrons", -1.2));

        questions.Add(Q(T("C1.1.1"), "An atom of carbon-14 has 6 protons. How many neutrons does it have?",
            D.Easy, "Mass number = protons + neutrons → 14 = 6 + n → n = 8.",
            "8", "6", "14", "12", -1.0));

        questions.Add(Q(T("C1.1.1"), "Which of the following is a polyatomic ion?",
            D.Easy, "SO₄²⁻ (sulfate) consists of multiple atoms bonded together carrying a charge.",
            "SO₄²⁻", "Na⁺", "Cl⁻", "Fe²⁺", -0.8));

        questions.Add(Q(T("C1.1.1"), "Isotopes of an element differ in:",
            D.Easy, "Isotopes have the same number of protons but different numbers of neutrons.",
            "Number of neutrons", "Number of protons", "Number of electrons", "Chemical properties", -1.0));

        // --- C1.1.2 Relative Atomic and Molecular Mass ---
        questions.Add(Q(T("C1.1.2"), "The relative molecular mass of H₂SO₄ is:",
            D.Easy, "Mr = 2(1) + 32 + 4(16) = 2 + 32 + 64 = 98.",
            "98", "96", "100", "80", -0.8));

        questions.Add(Q(T("C1.1.2"), "If the relative atomic mass of chlorine is 35.5, this means:",
            D.Medium, "Relative atomic mass is a weighted average of isotope masses based on natural abundance.",
            "It is the weighted average mass of chlorine isotopes relative to ¹²C", "All chlorine atoms have mass 35.5",
            "Chlorine has 35.5 neutrons", "Chlorine's molar mass is 35.5 g", 0.2));

        // --- C1.2.1 Chemical Formulas and Naming ---
        questions.Add(Q(T("C1.2.1"), "The chemical formula for aluminum oxide is:",
            D.Easy, "Al has charge +3, O has charge −2. Cross-over: Al₂O₃.",
            "Al₂O₃", "AlO₃", "Al₃O₂", "AlO", -0.8));

        questions.Add(Q(T("C1.2.1"), "What is the name of the compound FeCl₃?",
            D.Easy, "Fe³⁺ is iron(III), Cl⁻ is chloride → iron(III) chloride.",
            "Iron(III) chloride", "Iron(II) chloride", "Iron trichloride", "Ferric oxide", -0.5));

        // --- C1.2.2 Valence and Oxidation States ---
        questions.Add(Q(T("C1.2.2"), "In KMnO₄, the oxidation state of Mn is:",
            D.Medium, "K = +1, O = −2 each. 1 + Mn + 4(−2) = 0 → Mn = +7.",
            "+7", "+5", "+6", "+4", 0.3));

        questions.Add(Q(T("C1.2.2"), "The oxidation state of sulfur in Na₂S₂O₃ is:",
            D.Hard, "2(+1) + 2x + 3(−2) = 0 → 2 + 2x − 6 = 0 → x = +2.",
            "+2", "+4", "+6", "0", 1.0));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 2: CHEMICAL REACTIONS AND EQUATIONS (C2.x.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("C2.1.1"), "When balancing the equation: _Fe + _O₂ → _Fe₂O₃, the coefficients are:",
            D.Easy, "4Fe + 3O₂ → 2Fe₂O₃. Coefficients: 4, 3, 2.",
            "4, 3, 2", "2, 3, 1", "3, 2, 1", "1, 1, 1", -0.5));

        questions.Add(Q(T("C2.1.1"), "In the reaction 2H₂ + O₂ → 2H₂O, the mole ratio of H₂ to O₂ is:",
            D.Easy, "From the balanced equation, 2 mol H₂ reacts with 1 mol O₂.",
            "2 : 1", "1 : 2", "1 : 1", "4 : 1", -1.0));

        questions.Add(Q(T("C2.1.2"), "Which is an example of a decomposition reaction?",
            D.Easy, "2H₂O → 2H₂ + O₂ is decomposition: one compound breaks into simpler substances.",
            "2H₂O → 2H₂ + O₂", "2Na + Cl₂ → 2NaCl", "HCl + NaOH → NaCl + H₂O", "Fe + CuSO₄ → FeSO₄ + Cu", -0.8));

        questions.Add(Q(T("C2.1.2"), "A displacement reaction is characterized by:",
            D.Medium, "A more reactive element displaces a less reactive element from a compound.",
            "A more reactive element replacing a less reactive one in a compound",
            "Two compounds exchanging ions",
            "A single compound breaking into elements",
            "Elements combining to form a compound", 0.2));

        questions.Add(Q(T("C2.2.1"), "In the reaction 2Mg + O₂ → 2MgO, magnesium is:",
            D.Easy, "Mg goes from 0 to +2 (loses electrons) → oxidized. Mg is the reducing agent.",
            "Oxidized (it is the reducing agent)", "Reduced (it is the oxidizing agent)",
            "Neither oxidized nor reduced", "Both oxidized and reduced", -0.5));

        questions.Add(Q(T("C2.2.1"), "In a redox reaction, the substance that gains electrons is:",
            D.Easy, "The substance gaining electrons is reduced and acts as the oxidizing agent.",
            "The oxidizing agent", "The reducing agent", "The catalyst", "The solvent", -0.8));

        questions.Add(Q(T("C2.2.2"), "In the reaction Fe₂O₃ + 3CO → 2Fe + 3CO₂, the reducing agent is:",
            D.Medium, "CO is oxidized (C goes from +2 to +4) → CO is the reducing agent.",
            "CO", "Fe₂O₃", "Fe", "CO₂", 0.2));

        questions.Add(Q(T("C2.2.2"), "In 2FeCl₃ + Cu → 2FeCl₂ + CuCl₂, which species is reduced?",
            D.Medium, "Fe³⁺ gains an electron to become Fe²⁺ → Fe³⁺ is reduced.",
            "Fe³⁺", "Cu", "Cl⁻", "Cu²⁺", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 3: THE MOLE AND CHEMICAL CALCULATIONS (C3.x.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("C3.1.1"), "Avogadro's number (Nₐ) is approximately:",
            D.Easy, "Nₐ ≈ 6.022 × 10²³ mol⁻¹.",
            "6.022 × 10²³", "6.022 × 10²⁴", "6.022 × 10²²", "3.01 × 10²³", -1.2));

        questions.Add(Q(T("C3.1.1"), "How many molecules are in 2 mol of H₂O?",
            D.Easy, "N = nNₐ = 2 × 6.022 × 10²³ = 1.204 × 10²⁴.",
            "1.204 × 10²⁴", "6.022 × 10²³", "3.011 × 10²³", "1.204 × 10²²", -0.8));

        questions.Add(Q(T("C3.1.2"), "The molar mass of CaCO₃ is:",
            D.Easy, "M = 40 + 12 + 3(16) = 40 + 12 + 48 = 100 g/mol.",
            "100 g/mol", "84 g/mol", "110 g/mol", "60 g/mol", -0.8));

        questions.Add(Q(T("C3.1.2"), "How many moles are in 49 g of H₂SO₄?",
            D.Easy, "n = m/M = 49/98 = 0.5 mol.",
            "0.5 mol", "1 mol", "0.49 mol", "2 mol", -0.5));

        questions.Add(Q(T("C3.2.1"), "At STP, 1 mol of any ideal gas occupies:",
            D.Easy, "Standard molar volume at STP = 22.4 L/mol.",
            "22.4 L", "11.2 L", "44.8 L", "24.0 L", -1.0));

        questions.Add(Q(T("C3.2.1"), "What is the volume of 0.5 mol CO₂ at STP?",
            D.Easy, "V = nVm = 0.5 × 22.4 = 11.2 L.",
            "11.2 L", "22.4 L", "5.6 L", "44.8 L", -0.8));

        questions.Add(Q(T("C3.2.2"), "How many grams of O₂ are needed to fully combust 12 g of C?",
            D.Medium, "C + O₂ → CO₂. 12 g C = 1 mol C → needs 1 mol O₂ = 32 g.",
            "32 g", "16 g", "64 g", "48 g", 0.2));

        questions.Add(Q(T("C3.2.2"), "In 2H₂ + O₂ → 2H₂O, if 4 g H₂ reacts completely, how much H₂O is produced?",
            D.Medium, "4 g H₂ = 2 mol. 2 mol H₂ → 2 mol H₂O = 36 g.",
            "36 g", "18 g", "72 g", "9 g", 0.2));

        questions.Add(Q(T("C3.3.1"), "A 0.5 mol/L NaCl solution of 500 mL contains how many moles of NaCl?",
            D.Easy, "n = cV = 0.5 × 0.5 = 0.25 mol.",
            "0.25 mol", "0.5 mol", "1 mol", "0.1 mol", -0.5));

        questions.Add(Q(T("C3.3.2"), "100 mL of 2 mol/L HCl is diluted to 500 mL. The new concentration is:",
            D.Easy, "c₁V₁ = c₂V₂ → 2 × 100 = c₂ × 500 → c₂ = 0.4 mol/L.",
            "0.4 mol/L", "0.2 mol/L", "1 mol/L", "10 mol/L", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 4: ALKALI METALS AND THEIR COMPOUNDS (C4.x.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("C4.1.1"), "Sodium is stored under kerosene because:",
            D.Easy, "Sodium reacts vigorously with water and oxygen. Kerosene prevents contact with both.",
            "It reacts with water and oxygen in air", "It is highly flammable",
            "It dissolves in kerosene", "It reacts with kerosene slowly", -0.8));

        questions.Add(Q(T("C4.1.1"), "When sodium burns in excess oxygen, the product is:",
            D.Easy, "With excess O₂, sodium forms sodium peroxide: 2Na + O₂ → Na₂O₂.",
            "Na₂O₂ (sodium peroxide)", "Na₂O (sodium oxide)", "NaOH", "NaO₂", -0.5));

        questions.Add(Q(T("C4.1.1"), "Among Group IA metals, reactivity increases going down the group because:",
            D.Medium, "As atomic radius increases, the outermost electron is easier to remove (lower ionization energy).",
            "Ionization energy decreases with increasing atomic radius",
            "Electronegativity increases down the group",
            "The number of electron shells decreases",
            "Nuclear charge decreases", 0.3));

        questions.Add(Q(T("C4.1.2"), "Na₂CO₃ and NaHCO₃ can be distinguished by:",
            D.Medium, "NaHCO₃ decomposes on heating: 2NaHCO₃ → Na₂CO₃ + H₂O + CO₂. Na₂CO₃ does not decompose.",
            "Heating — NaHCO₃ decomposes, Na₂CO₃ does not",
            "Adding water — only one dissolves",
            "Color difference", "Adding NaCl solution", 0.3));

        questions.Add(Q(T("C4.1.2"), "NaOH is commonly known as:",
            D.Easy, "NaOH is called caustic soda or lye.",
            "Caustic soda", "Baking soda", "Washing soda", "Slaked lime", -1.0));

        questions.Add(Q(T("C4.2.1"), "The flame test color for sodium compounds is:",
            D.Easy, "Sodium produces a bright yellow flame.",
            "Yellow", "Violet (lilac)", "Red", "Green", -1.0));

        questions.Add(Q(T("C4.2.1"), "The flame test for potassium must be observed through cobalt blue glass because:",
            D.Medium, "The cobalt blue glass filters out the intense yellow sodium flame that would mask the lilac potassium color.",
            "To filter out yellow light from sodium impurities",
            "To amplify the potassium flame color",
            "Potassium flame is invisible to the naked eye",
            "To protect the eyes from UV radiation", 0.2));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 5: HALOGENS (C5.x.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("C5.1.1"), "Chlorine gas (Cl₂) is:",
            D.Easy, "Cl₂ is a yellow-green gas with a pungent, suffocating odor.",
            "A yellow-green gas with a pungent odor", "A colorless odorless gas",
            "A purple solid at room temperature", "A brown liquid", -0.8));

        questions.Add(Q(T("C5.1.1"), "At room temperature, the physical state of bromine (Br₂) is:",
            D.Easy, "Bromine is the only non-metal that is liquid at room temperature.",
            "Liquid (dark red-brown)", "Gas (yellow-green)", "Solid (purple-black)", "Solid (white)", -0.5));

        questions.Add(Q(T("C5.1.2"), "Among the halogens F₂, Cl₂, Br₂, I₂, the most reactive is:",
            D.Easy, "Reactivity of halogens decreases down the group. F₂ is the most reactive.",
            "F₂", "Cl₂", "Br₂", "I₂", -1.0));

        questions.Add(Q(T("C5.1.2"), "The reactivity trend of halogens decreases in the order F > Cl > Br > I because:",
            D.Medium, "As atomic radius increases down the group, the ability to attract electrons (electronegativity) decreases.",
            "Electronegativity decreases as atomic radius increases",
            "Ionization energy increases down the group",
            "The number of electron shells decreases",
            "Nuclear charge decreases", 0.3));

        questions.Add(Q(T("C5.2.1"), "HF is a weak acid while HCl, HBr, HI are strong acids. This is because:",
            D.Hard, "The H–F bond is exceptionally strong due to the small size and high electronegativity of fluorine, making it harder to dissociate.",
            "The H–F bond is very strong due to fluorine's small size",
            "Fluorine has fewer electron shells",
            "HF molecules are smaller",
            "Fluorine has lower electronegativity", 1.0));

        questions.Add(Q(T("C5.2.1"), "To test for the presence of chloride ions (Cl⁻) in solution, one adds:",
            D.Easy, "Add AgNO₃ solution; a white precipitate (AgCl) that is insoluble in dilute HNO₃ confirms Cl⁻.",
            "AgNO₃ solution followed by dilute HNO₃", "BaCl₂ solution",
            "NaOH solution", "Litmus indicator", -0.5));

        questions.Add(Q(T("C5.2.2"), "When Cl₂ is bubbled through KBr solution, the solution turns:",
            D.Medium, "Cl₂ + 2KBr → 2KCl + Br₂. The liberated Br₂ turns the solution orange-brown.",
            "Orange-brown (Br₂ is displaced)", "Colorless",
            "Yellow-green", "Purple", 0.2));

        questions.Add(Q(T("C5.2.2"), "In the reaction Cl₂ + 2NaBr → 2NaCl + Br₂, chlorine acts as:",
            D.Easy, "Cl₂ gains electrons (Cl₂ → 2Cl⁻) so it is reduced; it is the oxidizing agent.",
            "The oxidizing agent", "The reducing agent", "A catalyst", "A spectator species", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 6: SULFUR, NITROGEN, AND THEIR COMPOUNDS (C6.x.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("C6.1.1"), "Concentrated H₂SO₄ has all of the following properties EXCEPT:",
            D.Medium, "Concentrated H₂SO₄ is a strong oxidizer, dehydrating agent, and desiccant. It is NOT a reducing agent.",
            "Reducing agent", "Dehydrating agent", "Strong oxidizer (when hot and concentrated)", "Desiccant", 0.3));

        questions.Add(Q(T("C6.1.1"), "SO₂ is a common air pollutant that causes:",
            D.Easy, "SO₂ dissolves in rain to form sulfurous and sulfuric acid, leading to acid rain.",
            "Acid rain", "Ozone depletion", "Greenhouse effect", "Smog only", -0.8));

        questions.Add(Q(T("C6.1.2"), "In the Contact Process, SO₂ is converted to SO₃ using:",
            D.Medium, "V₂O₅ (vanadium pentoxide) is the catalyst in the Contact Process.",
            "V₂O₅ catalyst at 450°C", "Platinum catalyst at 800°C",
            "Iron catalyst at 500°C", "Concentrated H₂SO₄ catalyst", 0.3));

        questions.Add(Q(T("C6.1.2"), "The Contact Process produces sulfuric acid. The correct order of steps is:",
            D.Medium, "S → SO₂ (combustion) → SO₃ (catalytic oxidation) → H₂SO₄ (absorption in conc. H₂SO₄, then dilution).",
            "S → SO₂ → SO₃ → H₂SO₄", "S → SO₃ → SO₂ → H₂SO₄",
            "SO₃ → SO₂ → S → H₂SO₄", "S → H₂SO₄ → SO₂ → SO₃", 0.2));

        questions.Add(Q(T("C6.2.1"), "NH₃ (ammonia) is a gas that:",
            D.Easy, "NH₃ is colorless with a pungent smell, lighter than air, highly soluble in water.",
            "Is colorless with a pungent odor, highly soluble in water",
            "Is yellow-green with a suffocating odor",
            "Is odorless and insoluble in water",
            "Is heavier than air", -0.8));

        questions.Add(Q(T("C6.2.1"), "Ammonia acts as a base because:",
            D.Easy, "NH₃ has a lone pair on nitrogen that can accept a proton (H⁺) from water.",
            "It has a lone pair that can accept H⁺", "It releases OH⁻ directly",
            "It contains hydrogen atoms", "It reacts with metals", -0.3));

        questions.Add(Q(T("C6.2.2"), "Concentrated HNO₃ appears yellow because:",
            D.Medium, "Concentrated HNO₃ decomposes slightly in light: 4HNO₃ → 4NO₂ + 2H₂O + O₂. Dissolved NO₂ is brown/yellow.",
            "Dissolved NO₂ from decomposition", "Impurities in the acid",
            "The nitrogen atom absorbs blue light", "It contains dissolved sulfur", 0.3));

        questions.Add(Q(T("C6.3.1"), "In the Haber Process, the catalyst used is:",
            D.Easy, "The Haber Process uses an iron catalyst with K₂O/Al₂O₃ promoters.",
            "Iron (with promoters)", "Vanadium pentoxide", "Platinum", "Manganese dioxide", -0.5));

        questions.Add(Q(T("C6.3.1"), "The Haber Process operates at high pressure because:",
            D.Medium, "N₂ + 3H₂ ⇌ 2NH₃. More moles on left (4) than right (2); high pressure shifts equilibrium right (Le Chatelier).",
            "High pressure favors the side with fewer gas moles (products)",
            "High pressure increases the rate of decomposition of NH₃",
            "Gas molecules need more space to react",
            "It prevents the catalyst from deactivating", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 7: THE PERIODIC TABLE AND PERIODIC LAW (C7.x.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("C7.1.1"), "Elements in the same period have the same:",
            D.Easy, "Elements in the same period have the same number of electron shells.",
            "Number of electron shells", "Number of valence electrons",
            "Electronegativity", "Atomic radius", -1.0));

        questions.Add(Q(T("C7.1.1"), "Elements in the same group have the same:",
            D.Easy, "Elements in the same group have the same number of valence electrons.",
            "Number of valence electrons", "Number of electron shells",
            "Atomic mass", "Ionization energy", -0.8));

        questions.Add(Q(T("C7.1.2"), "Across a period from left to right, atomic radius generally:",
            D.Easy, "Increasing nuclear charge pulls electrons closer, so atomic radius decreases across a period.",
            "Decreases", "Increases", "Stays the same", "First increases then decreases", -0.5));

        questions.Add(Q(T("C7.1.2"), "First ionization energy generally increases across a period because:",
            D.Medium, "As nuclear charge increases with constant shielding, it becomes harder to remove an electron.",
            "Increasing effective nuclear charge holds electrons more tightly",
            "Atomic radius increases", "More electron shells are added",
            "Electronegativity decreases", 0.3));

        questions.Add(Q(T("C7.2.1"), "Which of the following elements has the strongest metallic character?",
            D.Easy, "Metallic character increases going down and to the left. Cs is the most metallic stable element.",
            "Cs", "Na", "Li", "Be", -0.8));

        questions.Add(Q(T("C7.2.1"), "Non-metallic character increases:",
            D.Easy, "Non-metallic character increases going up and to the right in the periodic table.",
            "Going up and to the right", "Going down and to the left",
            "Going down and to the right", "Going up and to the left", -0.5));

        questions.Add(Q(T("C7.2.2"), "Element X is in Period 3, Group VIIA. X is most likely:",
            D.Easy, "Period 3, Group VIIA is chlorine (Cl).",
            "Chlorine (Cl)", "Sodium (Na)", "Sulfur (S)", "Argon (Ar)", -0.8));

        questions.Add(Q(T("C7.2.2"), "An element has electron configuration 1s²2s²2p⁶3s²3p¹. It is:",
            D.Medium, "Total electrons = 13 → Aluminum (Al).",
            "Aluminum (Al)", "Silicon (Si)", "Magnesium (Mg)", "Phosphorus (P)", 0.2));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 8: CHEMICAL BONDING (C8.x.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("C8.1.1"), "Ionic bonds form between:",
            D.Easy, "Ionic bonds form between metals (which lose electrons) and non-metals (which gain electrons).",
            "A metal and a non-metal", "Two non-metals", "Two metals", "Two noble gases", -1.0));

        questions.Add(Q(T("C8.1.1"), "Which compound is ionic?",
            D.Easy, "NaCl consists of Na⁺ and Cl⁻ ions held by electrostatic attraction.",
            "NaCl", "CO₂", "H₂O", "CH₄", -0.8));

        questions.Add(Q(T("C8.1.2"), "A polar covalent bond forms when:",
            D.Easy, "In a polar covalent bond, electrons are shared unequally due to different electronegativities.",
            "Two atoms share electrons unequally", "Electrons are transferred completely",
            "Two atoms share electrons equally", "No electrons are involved", -0.5));

        questions.Add(Q(T("C8.1.2"), "Which molecule contains a non-polar covalent bond?",
            D.Easy, "In Cl₂, both atoms have the same electronegativity, so the bond is non-polar.",
            "Cl₂", "HCl", "H₂O", "NaCl", -0.8));

        questions.Add(Q(T("C8.2.1"), "Metallic bonding is best described as:",
            D.Medium, "In metallic bonding, delocalized electrons move freely among positive metal ion cores.",
            "A sea of delocalized electrons around positive metal ion cores",
            "Electron transfer from one metal atom to another",
            "Sharing of electron pairs between metal atoms",
            "Van der Waals forces between metal atoms", 0.2));

        questions.Add(Q(T("C8.2.2"), "According to VSEPR theory, the shape of CH₄ is:",
            D.Easy, "CH₄ has 4 bonding pairs and 0 lone pairs → tetrahedral.",
            "Tetrahedral", "Trigonal planar", "Linear", "Bent", -0.5));

        questions.Add(Q(T("C8.2.2"), "The molecular geometry of H₂O is:",
            D.Medium, "H₂O has 2 bonding pairs and 2 lone pairs → bent (V-shape).",
            "Bent (V-shape)", "Linear", "Tetrahedral", "Trigonal planar", 0.2));

        questions.Add(Q(T("C8.3.1"), "Hydrogen bonding occurs in which of the following?",
            D.Easy, "H₂O has O–H bonds where O is highly electronegative, enabling hydrogen bonding.",
            "H₂O", "CH₄", "CO₂", "Cl₂", -0.5));

        questions.Add(Q(T("C8.3.1"), "The boiling point of H₂O is unusually high compared to H₂S because:",
            D.Medium, "Water forms strong hydrogen bonds (O–H···O) that require extra energy to break.",
            "Water forms strong hydrogen bonds", "Water has a higher molar mass",
            "H₂S molecules are smaller", "Water is ionic", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 9: CHEMICAL REACTION RATES (C9.x.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("C9.1.1"), "Increasing temperature increases reaction rate because:",
            D.Easy, "Higher temperature means more kinetic energy, so more molecules exceed the activation energy.",
            "More molecules have energy ≥ activation energy", "The activation energy decreases",
            "The equilibrium constant changes", "Products become more stable", -0.5));

        questions.Add(Q(T("C9.1.1"), "Which of the following does NOT affect reaction rate?",
            D.Medium, "The enthalpy change (ΔH) tells us about energy released/absorbed, not the rate.",
            "Enthalpy change of the reaction", "Concentration of reactants",
            "Temperature", "Surface area of solid reactants", 0.3));

        questions.Add(Q(T("C9.1.1"), "Increasing the concentration of a reactant increases the reaction rate because:",
            D.Medium, "More particles per unit volume leads to more frequent effective collisions.",
            "More particles lead to more frequent collisions", "The activation energy is lowered",
            "The products decompose faster", "The catalyst becomes more effective", 0.2));

        questions.Add(Q(T("C9.1.2"), "According to collision theory, for a reaction to occur, colliding molecules must:",
            D.Medium, "Molecules must collide with sufficient energy (≥ Ea) and correct orientation.",
            "Have energy ≥ Ea and correct orientation", "Simply touch each other",
            "Be in the gas phase", "Have identical kinetic energies", 0.3));

        questions.Add(Q(T("C9.2.1"), "A catalyst speeds up a reaction by:",
            D.Easy, "A catalyst provides an alternative pathway with lower activation energy.",
            "Lowering the activation energy", "Increasing the temperature",
            "Increasing the concentration of reactants", "Changing the enthalpy of the reaction", -0.5));

        questions.Add(Q(T("C9.2.1"), "Which statement about catalysts is TRUE?",
            D.Medium, "Catalysts are not consumed in the reaction and can be recovered unchanged.",
            "They are not consumed in the reaction", "They always increase the yield of products",
            "They shift the equilibrium position", "They increase the activation energy", 0.2));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 10: CHEMICAL EQUILIBRIUM (C10.x.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("C10.1.1"), "A system is at dynamic equilibrium when:",
            D.Easy, "At equilibrium, the forward and reverse reaction rates are equal (but not zero).",
            "The forward and reverse rates are equal", "All reactions have stopped",
            "Only the forward reaction is occurring", "Concentrations of reactants are zero", -0.8));

        questions.Add(Q(T("C10.1.1"), "At equilibrium, the concentrations of reactants and products:",
            D.Easy, "At equilibrium, concentrations remain constant (but not necessarily equal).",
            "Remain constant but are not necessarily equal", "Are always equal",
            "Continue to change", "Are always zero for reactants", -0.5));

        questions.Add(Q(T("C10.1.2"), "For the reaction N₂ + 3H₂ ⇌ 2NH₃, if Kc is very large, this means:",
            D.Medium, "A large Kc means the equilibrium lies far to the right (products are favored).",
            "Products are strongly favored at equilibrium", "Reactants are strongly favored",
            "The reaction does not occur", "The forward rate is zero", 0.2));

        questions.Add(Q(T("C10.1.2"), "The units of Kc for the reaction 2SO₂ + O₂ ⇌ 2SO₃ are:",
            D.Hard, "Kc = [SO₃]²/([SO₂]²[O₂]) → (mol/L)²/((mol/L)²(mol/L)) = L/mol.",
            "L/mol (or mol⁻¹·L)", "mol²/L²", "No units", "mol/L", 1.0));

        questions.Add(Q(T("C10.2.1"), "According to Le Chatelier's principle, increasing pressure on N₂ + 3H₂ ⇌ 2NH₃ will:",
            D.Medium, "Increasing pressure shifts equilibrium toward the side with fewer moles of gas → products (2 mol vs 4 mol).",
            "Shift equilibrium toward NH₃ (products)", "Shift equilibrium toward N₂ and H₂",
            "Have no effect on equilibrium", "Stop the reaction completely", 0.3));

        questions.Add(Q(T("C10.2.1"), "Adding an inert gas at constant volume to an equilibrium system:",
            D.Medium, "Adding an inert gas at constant volume does not change the partial pressures or concentrations of reactants/products.",
            "Has no effect on the equilibrium position", "Shifts equilibrium to the right",
            "Shifts equilibrium to the left", "Increases the equilibrium constant", 0.5));

        questions.Add(Q(T("C10.2.2"), "For 2SO₂ + O₂ ⇌ 2SO₃, at equilibrium [SO₂]=0.2, [O₂]=0.1, [SO₃]=0.4 mol/L. Kc = ?",
            D.Medium, "Kc = (0.4)²/((0.2)²(0.1)) = 0.16/(0.04 × 0.1) = 0.16/0.004 = 40.",
            "40", "20", "4", "0.04", 0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 11: SOLUTIONS AND IONIC REACTIONS (C11.x.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("C11.1.1"), "A strong electrolyte is a substance that:",
            D.Easy, "Strong electrolytes dissociate completely into ions in solution.",
            "Completely dissociates into ions in solution", "Partially dissociates into ions",
            "Does not dissociate at all", "Conducts electricity only when solid", -0.8));

        questions.Add(Q(T("C11.1.1"), "Which is a strong electrolyte?",
            D.Easy, "NaCl completely dissociates in water: NaCl → Na⁺ + Cl⁻.",
            "NaCl", "CH₃COOH", "NH₃·H₂O", "C₂H₅OH", -0.5));

        questions.Add(Q(T("C11.1.2"), "The net ionic equation for the reaction NaOH + HCl → NaCl + H₂O is:",
            D.Easy, "Spectator ions (Na⁺, Cl⁻) are removed. Net: OH⁻ + H⁺ → H₂O.",
            "OH⁻ + H⁺ → H₂O", "Na⁺ + Cl⁻ → NaCl",
            "NaOH + HCl → NaCl + H₂O", "Na⁺ + OH⁻ + H⁺ + Cl⁻ → Na⁺ + Cl⁻ + H₂O", -0.5));

        questions.Add(Q(T("C11.1.2"), "In the ionic equation, spectator ions are:",
            D.Easy, "Spectator ions appear unchanged on both sides and do not participate in the reaction.",
            "Ions that remain unchanged and do not participate in the reaction",
            "Ions that are produced in the reaction",
            "Ions that act as catalysts",
            "Ions that precipitate out of solution", -0.8));

        questions.Add(Q(T("C11.2.1"), "A solution with pH = 3 is:",
            D.Easy, "pH < 7 is acidic. pH = 3 means [H⁺] = 10⁻³ mol/L.",
            "Acidic", "Basic", "Neutral", "Cannot be determined", -1.0));

        questions.Add(Q(T("C11.2.1"), "As [H⁺] increases, pH:",
            D.Easy, "pH = −log[H⁺]. As [H⁺] increases, pH decreases (more acidic).",
            "Decreases", "Increases", "Stays the same", "First increases then decreases", -0.8));

        questions.Add(Q(T("C11.2.2"), "When Na₂CO₃ dissolves in water, the solution is basic because:",
            D.Medium, "CO₃²⁻ hydrolyzes: CO₃²⁻ + H₂O ⇌ HCO₃⁻ + OH⁻, producing OH⁻ and making the solution basic.",
            "CO₃²⁻ hydrolyzes to produce OH⁻", "Na⁺ reacts with water",
            "Na₂CO₃ releases OH⁻ directly", "The solution contains undissociated NaOH", 0.3));

        questions.Add(Q(T("C11.3.1"), "Mixing AgNO₃ and NaCl solutions produces:",
            D.Easy, "Ag⁺ + Cl⁻ → AgCl↓ (white precipitate). AgCl is insoluble.",
            "A white precipitate of AgCl", "A yellow precipitate of AgI",
            "No reaction", "A gas is evolved", -0.8));

        questions.Add(Q(T("C11.3.1"), "According to solubility rules, which compound is soluble?",
            D.Easy, "All sodium compounds are soluble. Na₂SO₄ is soluble.",
            "Na₂SO₄", "BaSO₄", "PbCl₂", "AgCl", -0.5));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 12: ELECTROCHEMISTRY (C12.x.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("C12.1.1"), "In a galvanic (voltaic) cell, the anode is where:",
            D.Easy, "At the anode, oxidation occurs (the more reactive metal loses electrons).",
            "Oxidation occurs", "Reduction occurs", "No reaction occurs", "Electrons are gained", -0.5));

        questions.Add(Q(T("C12.1.1"), "In a Zn–Cu galvanic cell, which metal is the anode?",
            D.Easy, "Zinc is more reactive than copper, so Zn is oxidized at the anode.",
            "Zn (zinc)", "Cu (copper)", "Both equally", "Neither", -0.8));

        questions.Add(Q(T("C12.1.1"), "In a galvanic cell, electrons flow through the external circuit from:",
            D.Easy, "Electrons flow from the anode (oxidation) to the cathode (reduction) through the wire.",
            "Anode to cathode", "Cathode to anode", "In both directions equally", "Through the salt bridge", -0.5));

        questions.Add(Q(T("C12.1.2"), "A more positive standard electrode potential (E°) indicates:",
            D.Medium, "A more positive E° means a stronger tendency to be reduced (better oxidizing agent).",
            "A stronger tendency to be reduced", "A stronger tendency to be oxidized",
            "A faster reaction rate", "A higher activation energy", 0.3));

        questions.Add(Q(T("C12.2.1"), "During the electrolysis of molten NaCl, at the cathode:",
            D.Medium, "At the cathode (reduction): Na⁺ + e⁻ → Na. Sodium metal is deposited.",
            "Na is deposited (Na⁺ + e⁻ → Na)", "Cl₂ gas is produced",
            "O₂ gas is produced", "NaOH is formed", 0.3));

        questions.Add(Q(T("C12.2.1"), "Faraday's first law states that the mass deposited is proportional to:",
            D.Medium, "m = (M × I × t) / (n × F). Mass is proportional to the quantity of electricity (charge).",
            "The quantity of electricity (charge) passed", "The voltage applied",
            "The temperature of the electrolyte", "The concentration of the electrolyte", 0.3));

        questions.Add(Q(T("C12.2.2"), "Electroplating uses the principle of:",
            D.Easy, "Electroplating deposits a thin metal layer via electrolysis.",
            "Electrolysis", "Galvanic cell operation", "Nuclear fission", "Thermal decomposition", -0.5));

        questions.Add(Q(T("C12.2.2"), "In copper refining by electrolysis, the impure copper is the:",
            D.Medium, "The impure copper dissolves at the anode (oxidation: Cu → Cu²⁺ + 2e⁻), and pure copper deposits at the cathode.",
            "Anode", "Cathode", "Electrolyte", "Salt bridge", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 13: ORGANIC CHEMISTRY BASICS (C13.x.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("C13.1.1"), "The general formula for alkanes is:",
            D.Easy, "Alkanes have the general formula CₙH₂ₙ₊₂.",
            "CₙH₂ₙ₊₂", "CₙH₂ₙ", "CₙH₂ₙ₋₂", "CₙHₙ", -1.0));

        questions.Add(Q(T("C13.1.1"), "Structural isomers are molecules with the same molecular formula but different:",
            D.Easy, "Structural isomers have the same molecular formula but different structural arrangements.",
            "Structural arrangements of atoms", "Number of atoms",
            "Molecular mass", "Empirical formula", -0.5));

        questions.Add(Q(T("C13.1.1"), "How many structural isomers does C₄H₁₀ have?",
            D.Easy, "C₄H₁₀ has 2 isomers: n-butane and isobutane (2-methylpropane).",
            "2", "1", "3", "4", -0.5));

        questions.Add(Q(T("C13.1.2"), "Alkenes undergo addition reactions because they contain:",
            D.Easy, "The C=C double bond can open up to add atoms across it.",
            "A C=C double bond", "Only single bonds", "A triple bond", "A benzene ring", -0.8));

        questions.Add(Q(T("C13.1.2"), "The reaction CH₂=CH₂ + HBr → CH₃CH₂Br is an example of:",
            D.Easy, "HBr adds across the double bond — this is an addition reaction.",
            "Addition reaction", "Substitution reaction", "Elimination reaction", "Oxidation reaction", -0.5));

        questions.Add(Q(T("C13.1.2"), "Alkenes can be detected using:",
            D.Easy, "Bromine water is decolorized by alkenes (addition reaction), serving as a test.",
            "Bromine water (turns colorless)", "Litmus indicator",
            "Silver nitrate solution", "Sodium hydroxide solution", -0.5));

        questions.Add(Q(T("C13.2.1"), "The functional group of alcohols is:",
            D.Easy, "Alcohols contain the hydroxyl group –OH.",
            "–OH (hydroxyl)", "–COOH (carboxyl)", "–CHO (aldehyde)", "–NH₂ (amino)", -1.0));

        questions.Add(Q(T("C13.2.1"), "Ethanol (C₂H₅OH) can undergo dehydration to form:",
            D.Medium, "Intramolecular dehydration of ethanol at 170°C yields ethene: C₂H₅OH → C₂H₄ + H₂O.",
            "Ethene (C₂H₄)", "Ethane (C₂H₆)", "Diethyl ether", "Acetic acid", 0.2));

        questions.Add(Q(T("C13.2.2"), "The functional group of carboxylic acids is:",
            D.Easy, "Carboxylic acids contain the –COOH (carboxyl) group.",
            "–COOH (carboxyl)", "–OH (hydroxyl)", "–CHO (aldehyde)", "–CO– (ketone)", -0.8));

        questions.Add(Q(T("C13.2.2"), "Esterification is the reaction between:",
            D.Easy, "An alcohol and a carboxylic acid react to form an ester and water.",
            "An alcohol and a carboxylic acid", "Two alcohols",
            "An acid and a base", "An alkene and water", -0.5));

        questions.Add(Q(T("C13.3.1"), "A polymer formed by addition polymerization of ethene is:",
            D.Easy, "nCH₂=CH₂ → (–CH₂–CH₂–)ₙ = polyethylene.",
            "Polyethylene", "Nylon", "Polyester", "Starch", -0.5));

        questions.Add(Q(T("C13.3.1"), "Condensation polymerization differs from addition polymerization in that it:",
            D.Medium, "Condensation polymerization produces a small molecule (usually water) as a byproduct.",
            "Produces a small molecule (e.g., H₂O) as a byproduct",
            "Requires monomers with double bonds",
            "Produces no byproducts",
            "Only uses one type of monomer", 0.3));

        // ═══════════════════════════════════════════════════════════════
        // CHAPTER 14: CHEMISTRY EXPERIMENTS (C14.x.x)
        // ═══════════════════════════════════════════════════════════════

        questions.Add(Q(T("C14.1.1"), "A volumetric flask is used to:",
            D.Easy, "A volumetric flask is used to prepare solutions of precise concentration.",
            "Prepare solutions of precise concentration (volume)",
            "Measure the temperature of a solution",
            "Heat chemicals directly over a flame",
            "Collect gases", -0.8));

        questions.Add(Q(T("C14.1.1"), "When diluting concentrated H₂SO₄, one must:",
            D.Easy, "Always add acid to water (never water to acid) to prevent violent boiling and spattering.",
            "Add the acid slowly to water while stirring",
            "Add water to the acid quickly",
            "Heat the acid first",
            "Mix equal volumes at once", -0.5));

        questions.Add(Q(T("C14.1.2"), "Collecting a gas that is denser than air and does not react with water can be done by:",
            D.Easy, "Downward displacement of air: the dense gas sinks and pushes air out from above.",
            "Downward displacement of air", "Upward displacement of air",
            "Displacement of water only", "Condensation", -0.5));

        questions.Add(Q(T("C14.1.2"), "Hydrogen gas is collected by upward displacement of water or:",
            D.Easy, "H₂ is lighter than air, so it can be collected by downward displacement of water or upward displacement of air.",
            "Upward displacement of air (inverted container)", "Downward displacement of air",
            "Condensation", "Vacuum filtration", -0.3));

        questions.Add(Q(T("C14.2.1"), "In an acid-base titration, the indicator phenolphthalein turns pink in:",
            D.Easy, "Phenolphthalein is colorless in acid and pink in base (pH 8.2–10).",
            "Basic solution", "Acidic solution", "Neutral solution", "Any solution", -0.8));

        questions.Add(Q(T("C14.2.1"), "At the equivalence point of a strong acid–strong base titration, the pH is:",
            D.Easy, "Strong acid + strong base → salt + water. The salt of a strong acid and strong base is neutral (pH 7).",
            "7", "0", "14", "Depends on concentration", -0.5));

        questions.Add(Q(T("C14.2.2"), "To test for Fe³⁺ ions in solution, one adds:",
            D.Easy, "KSCN (potassium thiocyanate) gives a blood-red color with Fe³⁺.",
            "KSCN — a blood-red color confirms Fe³⁺",
            "NaOH — a white precipitate confirms Fe³⁺",
            "AgNO₃ — a yellow precipitate confirms Fe³⁺",
            "BaCl₂ — a white precipitate confirms Fe³⁺", -0.5));

        questions.Add(Q(T("C14.2.2"), "To test for SO₄²⁻ ions, one adds:",
            D.Easy, "Add BaCl₂; a white precipitate (BaSO₄) insoluble in dilute HCl confirms SO₄²⁻.",
            "BaCl₂ solution followed by dilute HCl", "AgNO₃ solution",
            "NaOH solution", "Litmus indicator", -0.5));

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
