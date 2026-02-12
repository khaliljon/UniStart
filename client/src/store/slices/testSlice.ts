import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { testService } from '../../services/testService';
import type { Question, AnswerResult, StartTestRequest } from '../../types';

interface TestState {
  currentQuestion: Question | null;
  selectedAnswer: number | null;
  answerResult: AnswerResult | null;
  questionsAnswered: number;
  testCompleted: boolean;
  isLoading: boolean;
  error: string | null;
}

const initialState: TestState = {
  currentQuestion: null,
  selectedAnswer: null,
  answerResult: null,
  questionsAnswered: 0,
  testCompleted: false,
  isLoading: false,
  error: null,
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
  async ({ questionId, answerOptionId }: { questionId: number; answerOptionId: number }) => {
    const response = await testService.submitAnswer({ questionId, answerOptionId });
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
    selectAnswer: (state, action: PayloadAction<number>) => {
      state.selectedAnswer = action.payload;
    },
    resetTest: (state) => {
      state.currentQuestion = null;
      state.selectedAnswer = null;
      state.answerResult = null;
      state.questionsAnswered = 0;
      state.testCompleted = false;
      state.error = null;
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
        state.answerResult = null;
        state.selectedAnswer = null;
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
        state.questionsAnswered += 1;
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
        state.testCompleted = false;
        state.error = null;
      })
      .addCase(resetTestProgress.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.error.message || 'Failed to reset progress';
      });
  },
});

export const { selectAnswer, resetTest, clearAnswerResult } = testSlice.actions;
export default testSlice.reducer;
