import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { examService } from '../../services/examService';
import type { ExamType, ExamSection } from '../../types';

interface ExamState {
  exams: ExamType[];
  sections: ExamSection[];
  selectedExams: string[];
  selectedSection: number | null;
  isLoading: boolean;
  error: string | null;
}

const initialState: ExamState = {
  exams: [],
  sections: [],
  selectedExams: [],
  selectedSection: null,
  isLoading: false,
  error: null,
};

export const fetchExams = createAsyncThunk('exam/fetchExams', async () => {
  const response = await examService.getExams();
  return response;
});

export const fetchExamSections = createAsyncThunk(
  'exam/fetchSections',
  async (examCode: string) => {
    const response = await examService.getExamSections(examCode);
    return response;
  }
);

const examSlice = createSlice({
  name: 'exam',
  initialState,
  reducers: {
    toggleExamSelection: (state, action: PayloadAction<string>) => {
      const examCode = action.payload;
      const index = state.selectedExams.indexOf(examCode);
      if (index === -1) {
        state.selectedExams.push(examCode);
      } else {
        state.selectedExams.splice(index, 1);
      }
    },
    setSelectedSection: (state, action: PayloadAction<number | null>) => {
      state.selectedSection = action.payload;
    },
    clearSelection: (state) => {
      state.selectedExams = [];
      state.selectedSection = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchExams.pending, (state) => {
        state.isLoading = true;
        state.error = null;
      })
      .addCase(fetchExams.fulfilled, (state, action: PayloadAction<ExamType[]>) => {
        state.isLoading = false;
        state.exams = action.payload;
      })
      .addCase(fetchExams.rejected, (state, action) => {
        state.isLoading = false;
        state.error = action.error.message || 'Failed to fetch exams';
      })
      .addCase(fetchExamSections.fulfilled, (state, action: PayloadAction<ExamSection[]>) => {
        state.sections = action.payload;
      });
  },
});

export const { toggleExamSelection, setSelectedSection, clearSelection } = examSlice.actions;
export default examSlice.reducer;
