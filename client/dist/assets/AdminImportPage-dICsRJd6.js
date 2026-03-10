import{r as i,j as r}from"./index-BqDfvEZt.js";import{a as p}from"./adminService-BdDbUUgE.js";function g(){const[o,l]=i.useState(""),[t,s]=i.useState(null),[n,d]=i.useState(!1),c=async()=>{try{d(!0),s(null);const e=JSON.parse(o),a=Array.isArray(e)?e:e.questions,m=await p.bulkImport(a);s(m)}catch(e){s({imported:0,failed:0,errors:[`JSON parse error: ${e}`]})}finally{d(!1)}};return r.jsxs("div",{className:"animate-fade-in",style:{padding:"2rem 0"},children:[r.jsx("h1",{style:{marginBottom:"0.5rem"},children:"Импорт вопросов"}),r.jsx("p",{style:{color:"var(--text-secondary)",marginBottom:"1.5rem"},children:"Массовый импорт вопросов из JSON"}),r.jsxs("div",{className:"card",style:{padding:"1.5rem"},children:[r.jsxs("p",{style:{color:"var(--text-secondary)",fontSize:"0.85rem",marginBottom:"1rem"},children:["Вставьте массив вопросов в формате JSON. Каждый вопрос должен содержать: ",r.jsx("code",{children:"topicId"}),", ",r.jsx("code",{children:"text"}),", ",r.jsx("code",{children:"difficulty"}),", ",r.jsx("code",{children:"answerOptions[]"}),"."]}),r.jsx("textarea",{value:o,onChange:e=>l(e.target.value),rows:14,placeholder:`[
  {
    "topicId": 1,
    "text": "What is 2+2?",
    "difficulty": "Easy",
    "explanation": "Basic addition",
    "answerOptions": [
      { "text": "3", "isCorrect": false },
      { "text": "4", "isCorrect": true },
      { "text": "5", "isCorrect": false },
      { "text": "6", "isCorrect": false }
    ]
  }
]`,style:{width:"100%",fontFamily:"monospace",fontSize:"0.85rem",padding:"0.75rem",borderRadius:"0.5rem",border:"1px solid var(--border-color)",background:"var(--background-color)",color:"var(--text-primary)",resize:"vertical"}}),r.jsx("button",{className:"btn btn-primary",style:{marginTop:"1rem"},disabled:n||!o.trim(),onClick:c,children:n?"Импорт…":"Импортировать"}),t&&r.jsxs("div",{style:{marginTop:"1rem",padding:"1rem",borderRadius:"0.5rem",background:t.failed>0?"rgba(239,68,68,0.08)":"rgba(16,185,129,0.08)",border:`1px solid ${t.failed>0?"var(--error-color)":"var(--success-color)"}`},children:[r.jsxs("div",{style:{fontWeight:600,marginBottom:"0.5rem"},children:["Импортировано: ",t.imported," / ",t.imported+t.failed]}),t.errors.length>0&&r.jsx("div",{style:{fontSize:"0.85rem",color:"var(--error-color)"},children:t.errors.map((e,a)=>r.jsxs("div",{children:["• ",e]},a))})]})]}),r.jsxs("div",{className:"card",style:{padding:"1.5rem",marginTop:"1rem"},children:[r.jsx("h3",{style:{marginBottom:"0.75rem"},children:"Формат вопроса"}),r.jsx("pre",{style:{background:"var(--background-color)",padding:"1rem",borderRadius:"0.5rem",fontSize:"0.8rem",overflow:"auto",color:"var(--text-primary)"},children:`{
  "topicId": number,       // ID темы (1-16)
  "text": string,          // Текст вопроса
  "difficulty": string,    // "Easy" | "Medium" | "Hard"
  "explanation": string?,  // Объяснение (опционально)
  "answerOptions": [       // Минимум 2, ровно 1 правильный
    { "text": string, "isCorrect": boolean }
  ]
}`}),r.jsx("p",{style:{color:"var(--text-secondary)",fontSize:"0.8rem",marginTop:"0.75rem"},children:"IRT-параметры (b, a, c) рассчитываются автоматически на основе difficulty."})]})]})}export{g as default};
