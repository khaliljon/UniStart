# API Examples

## Authentication

### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "student@example.com",
  "name": "John Doe",
  "password": "password123"
}
```

Response:
```json
{
  "userId": 1,
  "email": "student@example.com",
  "name": "John Doe",
  "role": "Student",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-02-12T10:30:00Z"
}
```

### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "student@example.com",
  "password": "password123"
}
```

## Exams

### Get All Exams
```http
GET /api/exams
Authorization: Bearer {token}
```

Response:
```json
[
  { "code": "SAT", "name": "SAT (Scholastic Assessment Test)" },
  { "code": "TOEFL", "name": "TOEFL (Test of English as a Foreign Language)" },
  { "code": "NUET", "name": "NUET (Nazarbayev University Entrance Test)" }
]
```

### Get Exam Sections
```http
GET /api/exams/SAT/sections
Authorization: Bearer {token}
```

Response:
```json
[
  { "id": 1, "examTypeCode": "SAT", "name": "Reading", "minScore": 200, "maxScore": 800 },
  { "id": 2, "examTypeCode": "SAT", "name": "Writing and Language", "minScore": 200, "maxScore": 800 },
  { "id": 3, "examTypeCode": "SAT", "name": "Math (No Calculator)", "minScore": 200, "maxScore": 800 },
  { "id": 4, "examTypeCode": "SAT", "name": "Math (Calculator)", "minScore": 200, "maxScore": 800 }
]
```

## Adaptive Test

### Get Next Question
```http
POST /api/test/next-question
Authorization: Bearer {token}
Content-Type: application/json

{
  "examTypeCodes": ["SAT"],
  "sectionId": null
}
```

Response:
```json
{
  "question": {
    "id": 1,
    "text": "Solve for x: 2x + 5 = 13",
    "difficulty": "Easy",
    "topicId": 4,
    "topicName": "Linear Equations",
    "options": [
      { "id": 1, "text": "x = 4" },
      { "id": 2, "text": "x = 5" },
      { "id": 3, "text": "x = 3" },
      { "id": 4, "text": "x = 6" }
    ]
  },
  "testCompleted": false,
  "questionsAnswered": 0,
  "totalQuestions": 0
}
```

### Submit Answer
```http
POST /api/test/submit-answer
Authorization: Bearer {token}
Content-Type: application/json

{
  "questionId": 1,
  "answerOptionId": 1
}
```

Response:
```json
{
  "isCorrect": true,
  "correctOptionId": 1,
  "newSkillLevel": 55,
  "skillChange": 5
}
```

### Get Skill Profiles
```http
GET /api/test/skill-profiles
Authorization: Bearer {token}
```

Response:
```json
[
  { "skillId": 1, "skillName": "Reading Comprehension", "skillCode": "READING_COMP", "level": 50, "lastUpdated": "2026-02-11T10:00:00Z" },
  { "skillId": 4, "skillName": "Algebra", "skillCode": "ALGEBRA", "level": 55, "lastUpdated": "2026-02-11T10:30:00Z" }
]
```

## Users

### Get User
```http
GET /api/users/1
Authorization: Bearer {token}
```

Response:
```json
{
  "id": 1,
  "email": "student@example.com",
  "name": "John Doe",
  "role": "Student",
  "createdAt": "2026-02-11T10:00:00Z"
}
```

### Update User
```http
PUT /api/users/1
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "John Smith",
  "email": null
}
```

## Analytics

### Get User Analytics
```http
GET /api/analytics
Authorization: Bearer {token}
```

Response:
```json
{
  "totalQuestionsAnswered": 25,
  "correctAnswers": 18,
  "overallAccuracy": 72.0,
  "skillProfiles": [
    { "skillId": 4, "skillName": "Algebra", "skillCode": "ALGEBRA", "level": 68, "lastUpdated": "2026-02-11T12:00:00Z" },
    { "skillId": 1, "skillName": "Reading Comprehension", "skillCode": "READING_COMP", "level": 55, "lastUpdated": "2026-02-11T11:30:00Z" }
  ],
  "recentProgress": [
    { "skillName": "Algebra", "oldLevel": 63, "newLevel": 68, "date": "2026-02-11T12:00:00Z" }
  ]
}
```

### Get Skills
```http
GET /api/analytics/skills
Authorization: Bearer {token}
```
