import React from 'react';
import AIFlashcardGenerator from '../../components/AIFlashcardGenerator';

const AdminAIFlashcardsPage: React.FC = () => {
  return (
    <div className="admin-page">
      <div className="breadcrumb">
        <span>Админ-панель</span>
        <span> / </span>
        <span>AI Генератор карточек</span>
      </div>
      
      <AIFlashcardGenerator />
    </div>
  );
};

export default AdminAIFlashcardsPage;
