# Database Schemas

## Tables
Users(id, email, name, role)
ExamTypes(code, name)
ExamSections(id, examTypeCode, name)
Skills(id, name)
Topics(id, skillId, name)
Questions(id, topicId, text, difficulty)
AnswerOptions(id, questionId, text, isCorrect)
UserAnswers(id, userId, questionId, answerOptionId)
UserSkillProfiles(userId, skillId, level)