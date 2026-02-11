# Domain Entities

## ExamType
- Code (SAT, TOEFL, NUET)
- Name

## ExamSection
- ExamType
- Name
- MinScore
- MaxScore

## Skill
- Code
- Name
- Description

## Topic
- Skill
- Section
...

Relationships:
ExamType 1→* ExamSection
Skill 1→* Topic
