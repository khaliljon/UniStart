import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { testService } from '../../services/testService';
import type { Question, AnswerResult, StartTestRequest } from '../../types';

interface TestState {
  currentQuestion: Question | null;
  selectedAnswer: number | null;
  answerResult: AnswerResult | null;
  questionsAnswered: number;
  totalQuestions: number;
  topicMastery: number;
  masteryReached: boolean;
  testCompleted: boolean;
  isLoading: boolean;
  error: string | null;
  questionStartTime: number | null; // timestamp when question was shown
  testSessionId: number | null;
}

const initialState: TestState = {
  currentQuestion: null,
  selectedAnswer: null,
  answerResult: null,
  questionsAnswered: 0,
  totalQuestions: 0,
  topicMastery: 0,
  masteryReached: false,
  testCompleted: false,
  isLoading: false,
  error: null,
  questionStartTime: null,
  testSessionId: null,
};

export const fetchNextQuestion = createAsyncThunk(
  'test/fetchNextQuestion',
  async (request: StartTestRequest) => {
    const response = await testService.getNextQuestion(request);
    return response;
  }
);

export const submitAnswer = createAsyncThunk(
  'test/submitAnswer',
  async ({ questionId, answerOptionId, timeSpentSeconds, testSessionId }: { questionId: number; answerOptionId: number; timeSpentSeconds?: number; testSessionId?: number }) => {
    const response = await testService.submitAnswer({ questionId, answerOptionId, timeSpentSeconds, testSessionId });
    return response;
  }
);

export const resetTestProgress = createAsyncThunk(
  'test/resetProgress',
  async () => {
    await testService.resetProgress();
  }
);

const testSlice = createSlice({
  name: 'test',
  initialState,
  reducers: {
    setTestSessionId: (state, action: PayloadAction<number | null>) => {
      state.testSessionId = action.payload;
    },
    selectAnswer: (state, action: PayloadAction<number>) => {
      state.selectedAnswer = action.payload;
    },
    resetTest: (state) => {
      state.currentQuestion = null;
      state.selectedAnswer = null;
      state.answerResult = null;
      state.questionsAnswered = 0;
      state.totalQuestions = 0;
      state.topicMastery = 0;
      state.masteryReached = false;
      state.testCompleted = false;
      state.isLoading = false;
      state.error = null;
      state.questionStartTime = null;
      state.testSessionId = null;
    },
    clearAnswerResult: (state) => {
      state.answerResult = null;
      state.selectedAnswer = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchNextQuestion.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchNextQuestion.fulfilled, (state, action) => {
        state.isLoading = false;
        state.currentQuestion = action.payload.question;
        state.testCompleted = action.payload.testCompleted;
        state.questionsAnswered = action.payload.questionsAnswered;
        state.totalQuestions = action.payload.totalQuestions;
        state.topicMastery = action.payload.topicMastery ?? 0;
        state.masteryReached = action.payload.masteryReached ?? false;
        state.answerResult = null;
        state.selectedAnswer = null;
        state.questionStartTime = Date.now();
      })
      .addCase(fetchNextQuestion.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.error.message || 'Failed to fetch question';
      })
      .addCase(submitAnswer.pending, (state) => {
        state.isLoading = true;
      })
      .addCase(submitAnswer.fulfilled, (state, action: PayloadAction<AnswerResult>) => {
        state.isLoading = false;
        state.answerResult = action.payload;
      })
      .addCase(submitAnswer.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.error.message || 'Failed to submit answer';
      })
      .addCase(resetTestProgress.pending, (state) => {
        state.isLoading = true;
      })
      .addCase(resetTestProgress.fulfilled, (state) => {
        state.isLoading = false;
        state.currentQuestion = null;
        state.selectedAnswer = null;
        state.answerResult = null;
        state.questionsAnswered = 0;
        state.totalQuestions = 0;
        state.topicMastery = 0;
        state.masteryReached = false;
        state.testCompleted = false;
        state.error = null;
        state.questionStartTime = null;
        state.testSessionId = null;
      })
      .addCase(resetTestProgress.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.error.message || 'Failed to reset progress';
      });
  },
});

export const { setTestSessionId, selectAnswer, resetTest, clearAnswerResult } = testSlice.actions;
export default testSlice.reducer;
