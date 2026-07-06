import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { examService } from '../../services/examService';
import type { ExamType, ExamSection } from '../../types';

interface ExamState {
  exams: ExamType[];
  sections: ExamSection[];
  selectedExams: string[];
  selectedSectionIds: number[];
  selectedSection: number | null;
  isLoading: boolean;
  error: string | null;
}

// Restore selectedExams from localStorage so they survive page refresh
const savedExams = (() => {
  try {
    const stored = localStorage.getItem('selectedExams');
    return stored ? JSON.parse(stored) as string[] : [];
  } catch { return []; }
})();

const savedSectionIds = (() => {
  try {
    const stored = localStorage.getItem('selectedSectionIds');
    return stored ? JSON.parse(stored) as number[] : [];
  } catch { return []; }
})();

const initialState: ExamState = {
  exams: [],
  sections: [],
  selectedExams: savedExams,
  selectedSectionIds: savedSectionIds,
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
      localStorage.setItem('selectedExams', JSON.stringify(state.selectedExams));
    },
    setSelectedExams: (state, action: PayloadAction<string[]>) => {
      state.selectedExams = action.payload;
      localStorage.setItem('selectedExams', JSON.stringify(action.payload));
    },
    setSelectedSectionIds: (state, action: PayloadAction<number[]>) => {
      state.selectedSectionIds = action.payload;
      localStorage.setItem('selectedSectionIds', JSON.stringify(action.payload));
    },
    setSelectedSection: (state, action: PayloadAction<number | null>) => {
      state.selectedSection = action.payload;
    },
    clearSelection: (state) => {
      state.selectedExams = [];
      state.selectedSectionIds = [];
      state.selectedSection = null;
      localStorage.removeItem('selectedExams');
      localStorage.removeItem('selectedSectionIds');
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
        // Reconcile persisted selections against the exams that actually exist in
        // the DB (admin-managed). Drops stale codes (e.g. a deleted NUET) so they
        // never show in the UI or trigger 404 section requests.
        const validCodes = new Set(action.payload.map((e) => e.code));
        const filtered = state.selectedExams.filter((c) => validCodes.has(c));
        if (filtered.length !== state.selectedExams.length) {
          state.selectedExams = filtered;
          localStorage.setItem('selectedExams', JSON.stringify(filtered));
        }
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

export const { toggleExamSelection, setSelectedExams, setSelectedSectionIds, setSelectedSection, clearSelection } = examSlice.actions;
export default examSlice.reducer;
