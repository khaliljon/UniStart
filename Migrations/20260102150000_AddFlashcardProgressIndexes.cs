using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UniStart.Migrations
{
    /// <inheritdoc />
    public partial class AddFlashcardProgressIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Удаляем устаревший индекс на Flashcard.NextReviewDate (если существует)
            migrationBuilder.DropIndex(
                name: "IX_Flashcards_NextReviewDate",
                table: "Flashcards");

            // Создаем индекс для быстрого поиска прогресса пользователя по карточке
            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardProgresses_UserId_FlashcardId",
                table: "UserFlashcardProgresses",
                columns: new[] { "UserId", "FlashcardId" },
                unique: true);

            // Создаем индекс для получения карточек к повторению (дата истекла)
            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardProgresses_UserId_NextReviewDate",
                table: "UserFlashcardProgresses",
                columns: new[] { "UserId", "NextReviewDate" });

            // Создаем индекс для подсчета освоенных карточек
            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardProgresses_UserId_IsMastered",
                table: "UserFlashcardProgresses",
                columns: new[] { "UserId", "IsMastered" });

            // Создаем индекс для доступа к наборам карточек
            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardSetAccesses_UserId_FlashcardSetId",
                table: "UserFlashcardSetAccesses",
                columns: new[] { "UserId", "FlashcardSetId" },
                unique: true);

            // Создаем индекс для поиска завершенных наборов
            migrationBuilder.CreateIndex(
                name: "IX_UserFlashcardSetAccesses_UserId_IsCompleted",
                table: "UserFlashcardSetAccesses",
                columns: new[] { "UserId", "IsCompleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Удаляем созданные индексы
            migrationBuilder.DropIndex(
                name: "IX_UserFlashcardProgresses_UserId_FlashcardId",
                table: "UserFlashcardProgresses");

            migrationBuilder.DropIndex(
                name: "IX_UserFlashcardProgresses_UserId_NextReviewDate",
                table: "UserFlashcardProgresses");

            migrationBuilder.DropIndex(
                name: "IX_UserFlashcardProgresses_UserId_IsMastered",
                table: "UserFlashcardProgresses");

            migrationBuilder.DropIndex(
                name: "IX_UserFlashcardSetAccesses_UserId_FlashcardSetId",
                table: "UserFlashcardSetAccesses");

            migrationBuilder.DropIndex(
                name: "IX_UserFlashcardSetAccesses_UserId_IsCompleted",
                table: "UserFlashcardSetAccesses");

            // Восстанавливаем устаревший индекс (для обратной совместимости)
            migrationBuilder.CreateIndex(
                name: "IX_Flashcards_NextReviewDate",
                table: "Flashcards",
                column: "NextReviewDate");
        }
    }
}
