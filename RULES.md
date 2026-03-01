# Adaptive Rules

## Skill Adjustment
- If user answers a question correctly => increase skill by +X
- If user answers incorrectly => decrease skill by -Y

## Question Selection
- If skill<40 => easier questions
- If skill>=40<70 => medium
- If skill>=70 => hard

## Exam Selection
- User can choose one or multiple exams
- Questions filtered by ExamType

## UI / Frontend Rules
- **No emojis/icons in UI text.** Do not use emoji characters (e.g. 📊, 🏠, 💬, ⭐) in navigation links, headings, buttons, labels, or any user-facing text within React components. Use plain text only. Functional Unicode symbols (e.g. ★ for star ratings, ✓ for checkmarks, ← → for arrows) are acceptable.
- Keep navigation menus compact: 4 main items maximum in the top navbar. Secondary links go into profile/more dropdowns.
