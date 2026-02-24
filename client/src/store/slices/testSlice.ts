import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { testService } from '../../services/testService';
import type { Question, AnswerResult, StartTestRequest, TestMode } from '../../types';

interface TestState {
  testMode: TestMode | null;
  currentQuestion: Question | null;
  selectedAnswer: number | null;
  answerResult: AnswerResult | null;
  questionsAnswered: number;
  totalQuestions: number;
  testCompleted: boolean;
  isLoading: boolean;
  error: string | null;
  timeRemaining: number | null; // seconds remaining for exam mode
  questionStartTime: number | null; // timestamp when question was shown
  testSessionId: number | null;
}

const initialState: TestState = {
  testMode: null,
  currentQuestion: null,
  selectedAnswer: null,
  answerResult: null,
  questionsAnswered: 0,
  totalQuestions: 0,
  testCompleted: false,
  isLoading: false,
  error: null,
  timeRemaining: null,
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
    setTestMode: (state, action: PayloadAction<TestMode>) => {
      state.testMode = action.payload;
      if (action.payload === 'exam') {
        state.timeRemaining = 60;
      } else {
        state.timeRemaining = null;
      }
    },
    setTestSessionId: (state, action: PayloadAction<number | null>) => {
      state.testSessionId = action.payload;
    },
    selectAnswer: (state, action: PayloadAction<number>) => {
      state.selectedAnswer = action.payload;
    },
    decrementTimer: (state) => {
      if (state.timeRemaining !== null && state.timeRemaining > 0) {
        state.timeRemaining -= 1;
      }
    },
    resetTimer: (state) => {
      if (state.testMode === 'exam') {
        state.timeRemaining = 60;
      }
    },
    resetTest: (state) => {
      state.testMode = null;
      state.currentQuestion = null;
      state.selectedAnswer = null;
      state.answerResult = null;
      state.questionsAnswered = 0;
      state.totalQuestions = 0;
      state.testCompleted = false;
      state.error = null;
      state.timeRemaining = null;
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
        state.testMode = null;
        state.currentQuestion = null;
        state.selectedAnswer = null;
        state.answerResult = null;
        state.questionsAnswered = 0;
        state.totalQuestions = 0;
        state.testCompleted = false;
        state.error = null;
        state.timeRemaining = null;
        state.questionStartTime = null;
        state.testSessionId = null;
      })
      .addCase(resetTestProgress.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.error.message || 'Failed to reset progress';
      });
  },
});

export const { setTestMode, setTestSessionId, selectAnswer, decrementTimer, resetTimer, resetTest, clearAnswerResult } = testSlice.actions;
export default testSlice.reducer;
