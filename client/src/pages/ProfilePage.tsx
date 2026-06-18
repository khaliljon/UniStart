import { useEffect, useState, FormEvent } from 'react';
import { useAppSelector } from '../hooks/useAppSelector';
import { useAppDispatch } from '../hooks/useAppDispatch';
import { useTranslation } from '../hooks/useTranslation';
import { fetchExams, fetchExamSections, toggleExamSelection, setSelectedSectionIds } from '../store/slices/examSlice';
import { completeProfile } from '../store/slices/authSlice';
import { subscriptionService } from '../services/subscriptionService';
import { tutorService } from '../services/tutorService';
import { referralService, type ReferralStats } from '../services/referralService';
import { authService } from '../services/authService';
import api from '../services/api';
import { useToast } from '../components/Toast';
import { PricingModal } from '../components/PricingModal';
import type { SubscriptionStatus, ExamSection, LinkedTutorInfo } from '../types';

function ProfilePage() {
  const { user } = useAppSelector((state) => state.auth);
  const { exams, selectedExams, selectedSectionIds } = useAppSelector((state) => state.exam);
  const dispatch = useAppDispatch();
  const { t } = useTranslation();
  const { showToast } = useToast();
  const [sub, setSub] = useState<SubscriptionStatus | null>(null);
  const [showPricing, setShowPricing] = useState(false);
  const [allSections, setAllSections] = useState<ExamSection[]>([]);
  const [linkedTutor, setLinkedTutor] = useState<LinkedTutorInfo | null>(null);
  const [inviteCode, setInviteCode] = useState('');
  const [linkLoading, setLinkLoading] = useState(false);
  const [linkError, setLinkError] = useState<string | null>(null);
  const [unlinkLoading, setUnlinkLoading] = useState(false);
  const isStudent = user?.role === 'Student';

  // Referral program
  const [refStats, setRefStats] = useState<ReferralStats | null>(null);
  const [refActivating, setRefActivating] = useState(false);
  const [refCopied, setRefCopied] = useState(false);

  // Password change
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [pwdError, setPwdError] = useState<string | null>(null);
  const [pwdSuccess, setPwdSuccess] = useState<string | null>(null);
  const [pwdLoading, setPwdLoading] = useState(false);

  // Email change
  const [newEmail, setNewEmail] = useState('');
  const [emailPassword, setEmailPassword] = useState('');
  const [emailError, setEmailError] = useState<string | null>(null);
  const [emailSuccess, setEmailSuccess] = useState<string | null>(null);
  const [emailLoading, setEmailLoading] = useState(false);

  // Name change
  const [editFirstName, setEditFirstName] = useState(user?.firstName || '');
  const [editLastName, setEditLastName] = useState(user?.lastName || '');
  const [nameError, setNameError] = useState<string | null>(null);
  const [nameSuccess, setNameSuccess] = useState<string | null>(null);
  const [nameLoading, setNameLoading] = useState(false);

  // Phone change
  const [editPhone, setEditPhone] = useState(user?.phoneNumber || '+7');
  const [phoneError, setPhoneError] = useState<string | null>(null);
  const [phoneSuccess, setPhoneSuccess] = useState<string | null>(null);
  const [phoneLoading, setPhoneLoading] = useState(false);

  useEffect(() => {
    dispatch(fetchExams());
    subscriptionService.getStatus().then(setSub).catch(() => {});
    if (user?.role === 'Student') {
      tutorService.getMyTutor().then(setLinkedTutor).catch(() => {});
    }
    referralService.getStats().then(setRefStats).catch(() => {});
  }, [dispatch]);

  // Load sections for all selected exams
  useEffect(() => {
    if (selectedExams.length === 0) {
      setAllSections([]);
      return;
    }
    Promise.all(
      selectedExams.map(code =>
        dispatch(fetchExamSections(code)).unwrap().catch(() => [] as ExamSection[])
      )
    ).then(results => {
      const flat = results.flat();
      setAllSections(flat);
      // Auto-cleanup: keep only IDs that belong to current exams
      const validIds = new Set(flat.map(s => s.id));
      const cleaned = selectedSectionIds.filter(id => validIds.has(id));
      // If user had no valid selections, auto-select all
      if (cleaned.length === 0 && flat.length > 0) {
        dispatch(setSelectedSectionIds(flat.map(s => s.id)));
      } else if (cleaned.length !== selectedSectionIds.length) {
        dispatch(setSelectedSectionIds(cleaned));
      }
    });
  }, [selectedExams, dispatch]);

  const handleToggleSection = (sectionId: number) => {
    const next = selectedSectionIds.includes(sectionId)
      ? selectedSectionIds.filter(id => id !== sectionId)
      : [...selectedSectionIds, sectionId];
    // Don't allow deselecting all sections for an exam
    if (next.length === 0) return;
    dispatch(setSelectedSectionIds(next));
  };

  const isPro = user?.subscriptionTier === 'Pro';

  const handleLinkTutor = async () => {
    if (!inviteCode.trim()) return;
    setLinkLoading(true);
    setLinkError(null);
    try {
      const result = await tutorService.linkByInviteCode(inviteCode.trim());
      if (result.success) {
        const tutor = await tutorService.getMyTutor();
        setLinkedTutor(tutor);
        setInviteCode('');
        showToast(t.profilePage.tutorLinked);
        subscriptionService.getStatus().then(setSub).catch(() => {});
      } else {
        setLinkError(result.message);
        showToast(result.message, 'error');
      }
    } catch {
      setLinkError('Ошибка привязки');
      showToast('Ошибка привязки', 'error');
    } finally {
      setLinkLoading(false);
    }
  };

  const handleUnlinkTutor = async () => {
    if (!confirm(t.profilePage.confirmUnlink)) return;
    setUnlinkLoading(true);
    try {
      await tutorService.unlinkFromTutor();
      setLinkedTutor(null);
      subscriptionService.getStatus().then(setSub).catch(() => {});
    } catch {
      alert('Ошибка отвязки');
    } finally {
      setUnlinkLoading(false);
    }
  };

  const handleActivateReferral = async () => {
    setRefActivating(true);
    try {
      const res = await referralService.activate();
      setRefStats(prev => prev ? { ...prev, code: res.code, isActive: true } : null);
      referralService.getStats().then(setRefStats).catch(() => {});
    } catch { /* ignore */ } finally { setRefActivating(false); }
  };

  const handleCopyReferral = () => {
    if (!refStats?.code) return;
    const link = `${window.location.origin}/register?ref=${refStats.code}`;
    navigator.clipboard.writeText(link).then(() => {
      setRefCopied(true);
      setTimeout(() => setRefCopied(false), 2000);
    });
  };

  const handleChangePassword = async (e: FormEvent) => {
    e.preventDefault();
    setPwdError(null);
    setPwdSuccess(null);
    if (newPassword !== confirmPassword) { setPwdError(t.profilePage.passwordsDoNotMatch); return; }
    try {
      setPwdLoading(true);
      await authService.changePassword({ currentPassword, newPassword });
      setPwdSuccess(t.profilePage.passwordChanged);
      setCurrentPassword(''); setNewPassword(''); setConfirmPassword('');
    } catch (err: unknown) {
      setPwdError((err as { response?: { data?: { error?: string } } })?.response?.data?.error || t.profilePage.passwordChangeError);
    } finally { setPwdLoading(false); }
  };

  const handleChangeEmail = async (e: FormEvent) => {
    e.preventDefault();
    setEmailError(null);
    setEmailSuccess(null);
    if (!newEmail.includes('@')) { setEmailError('Invalid email'); return; }
    try {
      setEmailLoading(true);
      await authService.changeEmail({ newEmail, password: emailPassword });
      setEmailSuccess(t.profilePage.emailChanged);
      setNewEmail(''); setEmailPassword('');
    } catch (err: unknown) {
      setEmailError((err as { response?: { data?: { error?: string } } })?.response?.data?.error || t.profilePage.emailChangeError);
    } finally { setEmailLoading(false); }
  };

  const handleChangeName = async (e: FormEvent) => {
    e.preventDefault();
    setNameError(null);
    setNameSuccess(null);
    if (editFirstName.trim().length < 2) { setNameError('First name must be at least 2 characters'); return; }
    if (/\d/.test(editFirstName)) { setNameError('First name must not contain digits'); return; }
    if (editLastName.trim().length < 2) { setNameError('Last name must be at least 2 characters'); return; }
    if (/\d/.test(editLastName)) { setNameError('Last name must not contain digits'); return; }
    try {
      setNameLoading(true);
      await api.put(`/users/${user?.id}`, { firstName: editFirstName.trim(), lastName: editLastName.trim() });
      setNameSuccess(t.common.save + ' ✓');
      // Update localStorage user
      if (user) {
        const updated = { ...user, firstName: editFirstName.trim(), lastName: editLastName.trim(), name: `${editFirstName.trim()} ${editLastName.trim()}`.trim() };
        localStorage.setItem('user', JSON.stringify(updated));
      }
    } catch (err: unknown) {
      setNameError((err as { response?: { data?: { error?: string } } })?.response?.data?.error || 'Error');
    } finally { setNameLoading(false); }
  };

  const handleChangePhone = async (e: FormEvent) => {
    e.preventDefault();
    setPhoneError(null);
    setPhoneSuccess(null);
    if (!/^\+77\d{9}$/.test(editPhone)) { setPhoneError(t.auth.phoneInvalid); return; }
    try {
      setPhoneLoading(true);
      await dispatch(completeProfile(editPhone)).unwrap();
      setPhoneSuccess(t.common.save + ' ✓');
    } catch (err: unknown) {
      setPhoneError(typeof err === 'string' ? err : t.auth.phoneInvalid);
    } finally { setPhoneLoading(false); }
  };

  return (
    <>
    <div className="animate-fade-in" style={{ maxWidth: '640px', margin: '0 auto', padding: '2rem 0' }}>
      <h1 style={{ marginBottom: '1.5rem' }}>{t.profilePage.title}</h1>

      {/* ─── User Info Card ─── */}
      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginBottom: '1rem' }}>
          <div style={{
            width: '56px', height: '56px', borderRadius: '50%',
            background: 'var(--primary-color)', color: '#fff',
            display: 'flex', alignItems: 'center', justifyContent: 'center',
            fontSize: '1.25rem', fontWeight: 700, flexShrink: 0,
          }}>
            {user?.name?.split(' ').map((w: string) => w[0]).join('').toUpperCase().slice(0, 2) || '?'}
          </div>
          <div>
            <h2 style={{ margin: 0, fontSize: '1.2rem' }}>{user?.name}</h2>
            <p style={{ margin: '0.15rem 0 0', color: 'var(--text-secondary)', fontSize: '0.9rem' }}>{user?.email}</p>
          </div>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '0.75rem', fontSize: '0.85rem' }}>
          <InfoRow label={t.profilePage.personalInfo} value={user?.role === 'Student' ? t.nav.home : user?.role === 'Tutor' ? t.tutor.editProfile : user?.role || '—'} />
          <InfoRow label={t.profilePage.registeredAt} value={user?.createdAt ? new Date(user.createdAt).toLocaleDateString() : '—'} />
        </div>
      </div>

      {/* ─── Edit Name ─── */}
      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
        <h3 style={{ margin: '0 0 1rem', fontSize: '1rem' }}>{t.auth.firstName} / {t.auth.lastName}</h3>
        {nameError && <div style={{ color: 'var(--error-color)', marginBottom: '0.75rem', padding: '0.5rem 0.75rem', background: 'var(--error-bg)', borderRadius: '6px', fontSize: '0.85rem' }}>{nameError}</div>}
        {nameSuccess && <div style={{ color: 'var(--success-color)', marginBottom: '0.75rem', padding: '0.5rem 0.75rem', background: 'rgba(16,185,129,0.08)', borderRadius: '6px', fontSize: '0.85rem' }}>✓ {nameSuccess}</div>}
        <form onSubmit={handleChangeName} style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          <div>
            <label className="form-label">{t.auth.firstName}</label>
            <input type="text" className="form-input" value={editFirstName} onChange={e => setEditFirstName(e.target.value)} style={{ width: '100%' }} />
          </div>
          <div>
            <label className="form-label">{t.auth.lastName}</label>
            <input type="text" className="form-input" value={editLastName} onChange={e => setEditLastName(e.target.value)} style={{ width: '100%' }} />
          </div>
          <button type="submit" className="btn btn-primary" disabled={nameLoading || !editFirstName.trim() || !editLastName.trim()} style={{ alignSelf: 'flex-start', fontSize: '0.9rem' }}>
            {nameLoading ? '...' : t.common.save}
          </button>
        </form>
      </div>

      {/* ─── Edit Phone ─── */}
      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
        <h3 style={{ margin: '0 0 1rem', fontSize: '1rem' }}>{t.auth.phone}</h3>
        {phoneError && <div style={{ color: 'var(--error-color)', marginBottom: '0.75rem', padding: '0.5rem 0.75rem', background: 'var(--error-bg)', borderRadius: '6px', fontSize: '0.85rem' }}>{phoneError}</div>}
        {phoneSuccess && <div style={{ color: 'var(--success-color)', marginBottom: '0.75rem', padding: '0.5rem 0.75rem', background: 'rgba(16,185,129,0.08)', borderRadius: '6px', fontSize: '0.85rem' }}>✓ {phoneSuccess}</div>}
        <form onSubmit={handleChangePhone} style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          <div>
            <label className="form-label">{t.auth.phone}</label>
            <input
              type="tel"
              className="form-input"
              value={editPhone}
              onChange={e => {
                let digits = e.target.value.replace(/\D/g, '');
                if (digits.startsWith('8')) digits = '7' + digits.slice(1);
                if (!digits.startsWith('7')) digits = '7' + digits;
                setEditPhone('+' + digits.slice(0, 11));
              }}
              placeholder="+7 700 123 45 67"
              maxLength={12}
              style={{ width: '100%' }}
            />
          </div>
          <button type="submit" className="btn btn-primary" disabled={phoneLoading || !editPhone.trim()} style={{ alignSelf: 'flex-start', fontSize: '0.9rem' }}>
            {phoneLoading ? '...' : t.common.save}
          </button>
        </form>
      </div>

      {/* ─── Subscription (student only) ─── */}
      {isStudent && (
      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
        <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>{t.profilePage.subscription}</h3>
        <div style={{
          display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.75rem',
        }}>
          <span style={{
            padding: '0.3rem 0.75rem', borderRadius: '999px', fontWeight: 700, fontSize: '0.85rem',
            background: isPro ? 'linear-gradient(135deg, #f59e0b, #ef4444)' : 'var(--bg-secondary)',
            color: isPro ? '#fff' : 'var(--text-secondary)',
          }}>
            {isPro ? t.profilePage.pro : t.profilePage.free}
          </span>
          {isPro && user?.subscriptionExpiresAt && (
            <span style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
              {t.profilePage.validUntil.replace('{date}', new Date(user.subscriptionExpiresAt).toLocaleDateString())}
            </span>
          )}
        </div>

        {sub && (
          <div style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>
            <p style={{ margin: '0.25rem 0' }}>{t.limits.questionsToday}: {sub.dailyUsage.questionsAnswered} / {sub.dailyUsage.questionsLimit === -1 ? '∞' : sub.dailyUsage.questionsLimit}</p>
          </div>
        )}

        {!isPro && (
          <button className="btn btn-primary" style={{ marginTop: '0.75rem', fontSize: '0.85rem' }}
            onClick={() => setShowPricing(true)}>
            {t.profilePage.upgradePro}
          </button>
        )}
      </div>
      )}

      {/* ─── My Tutor (student only) ─── */}
      {isStudent && (
        <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
          <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>{t.profilePage.myTutor}</h3>

          {linkedTutor ? (
            <div>
              <div style={{
                display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.75rem',
                padding: '0.75rem', borderRadius: '10px', background: 'var(--bg-secondary)',
              }}>
                <div style={{
                  width: '48px', height: '48px', borderRadius: '50%',
                  background: 'linear-gradient(135deg, #10b981, #059669)',
                  display: 'flex', alignItems: 'center', justifyContent: 'center',
                  color: '#fff', fontWeight: 700, fontSize: '1rem', flexShrink: 0,
                }}>
                  {(linkedTutor.tutorName || '').split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2)}
                </div>
                <div style={{ flex: 1 }}>
                  <div style={{ fontWeight: 600 }}>{linkedTutor.tutorName}</div>
                  <div style={{ fontSize: '0.8rem', color: 'var(--text-secondary)' }}>
                    {linkedTutor.headline}
                    {linkedTutor.averageRating > 0 && ` · ★ ${linkedTutor.averageRating.toFixed(1)}`}
                  </div>
                  {linkedTutor.specializations.length > 0 && (
                    <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)', marginTop: '0.2rem' }}>
                      {linkedTutor.specializations.join(', ')}
                    </div>
                  )}
                </div>
              </div>

              {sub?.hasTutorDiscount && (
                <div style={{
                  padding: '0.5rem 0.75rem', borderRadius: '8px', marginBottom: '0.75rem',
                  background: 'rgba(16, 185, 129, 0.1)', border: '1px solid rgba(16, 185, 129, 0.3)',
                  fontSize: '0.85rem', color: '#10b981', fontWeight: 600,
                }}>
                  ✓ {t.profilePage.tutorDiscount}
                </div>
              )}

              <button
                className="btn btn-outline"
                onClick={handleUnlinkTutor}
                disabled={unlinkLoading}
                style={{ fontSize: '0.85rem', color: '#ef4444', borderColor: '#ef4444' }}
              >
                {unlinkLoading ? t.profilePage.unlinking : t.profilePage.unlinkFromTutor}
              </button>
            </div>
          ) : (
            <div>
              <p style={{ color: 'var(--text-secondary)', fontSize: '0.85rem', margin: '0 0 0.75rem' }}>
                {t.profilePage.noTutorLinkedDesc}
              </p>
              <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center', flexWrap: 'wrap' }}>
                <input
                  type="text"
                  value={inviteCode}
                  onChange={e => { setInviteCode(e.target.value.toUpperCase()); setLinkError(null); }}
                  placeholder={t.profilePage.inviteCodePlaceholder}
                  maxLength={10}
                  style={{
                    padding: '0.5rem 0.75rem', borderRadius: '8px',
                    border: '2px solid var(--border-color)', background: 'var(--bg-secondary)',
                    color: 'var(--text-primary)', fontSize: '1rem', fontFamily: 'monospace',
                    letterSpacing: '0.1em', width: '160px', textTransform: 'uppercase',
                  }}
                />
                <button
                  className="btn btn-primary"
                  onClick={handleLinkTutor}
                  disabled={linkLoading || !inviteCode.trim()}
                  style={{ padding: '0.5rem 1rem', fontSize: '0.85rem' }}
                >
                  {linkLoading ? t.profilePage.linking : t.profilePage.linkToTutor}
                </button>
              </div>
              {linkError && (
                <p style={{ color: '#ef4444', fontSize: '0.82rem', marginTop: '0.5rem' }}>{linkError}</p>
              )}
            </div>
          )}
        </div>
      )}

      {/* ─── Referral Program ─── */}
      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
        <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>{t.profilePage.referralTitle}</h3>
        {refStats?.code ? (
          <div>
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.75rem', flexWrap: 'wrap' }}>
              <span style={{ fontSize: '0.85rem', color: 'var(--text-secondary)' }}>{t.profilePage.referralYourCode}:</span>
              <code style={{
                background: 'var(--primary-color)', color: '#fff', padding: '0.3rem 0.6rem',
                borderRadius: '6px', fontWeight: 700, letterSpacing: '0.1em', fontSize: '0.95rem',
              }}>
                {refStats.code}
              </code>
              <button className="btn btn-outline" onClick={handleCopyReferral} style={{ padding: '0.3rem 0.6rem', fontSize: '0.8rem' }}>
                {refCopied ? '✓' : t.profilePage.referralCopy}
              </button>
            </div>

            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(120px, 1fr))', gap: '0.5rem', marginBottom: '0.75rem' }}>
              <div style={{ background: 'var(--background-color)', borderRadius: '8px', padding: '0.6rem', textAlign: 'center' }}>
                <div style={{ fontSize: '1.25rem', fontWeight: 700, color: 'var(--primary-color)' }}>{refStats.totalReferred}</div>
                <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>{t.profilePage.referralReferred}</div>
              </div>
              <div style={{ background: 'var(--background-color)', borderRadius: '8px', padding: '0.6rem', textAlign: 'center' }}>
                <div style={{ fontSize: '1.25rem', fontWeight: 700, color: 'var(--primary-color)' }}>{refStats.totalPaid}</div>
                <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>{t.profilePage.referralPaid}</div>
              </div>
              <div style={{ background: 'var(--background-color)', borderRadius: '8px', padding: '0.6rem', textAlign: 'center' }}>
                <div style={{ fontSize: '1.25rem', fontWeight: 700, color: 'var(--primary-color)' }}>
                  {refStats.rewardType === 'money' ? `${refStats.totalEarned} ₸` : `+${refStats.bonusDays} ${t.profilePage.referralDays}`}
                </div>
                <div style={{ fontSize: '0.75rem', color: 'var(--text-secondary)' }}>{t.profilePage.referralEarned}</div>
              </div>
            </div>

            <p style={{ fontSize: '0.8rem', color: 'var(--text-secondary)', margin: 0 }}>
              {refStats.rewardType === 'money'
                ? t.profilePage.referralMoneyDesc
                : t.profilePage.referralDaysDesc}
              {' '}<a href="/referral-terms" style={{ color: 'var(--primary-color)' }}>{t.profilePage.referralTermsLink}</a>
            </p>
          </div>
        ) : (
          <div>
            <p style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', marginBottom: '0.75rem' }}>
              {t.profilePage.referralInactiveDesc}
            </p>
            <button className="btn btn-primary" onClick={handleActivateReferral} disabled={refActivating}
              style={{ padding: '0.5rem 1rem', fontSize: '0.85rem' }}>
              {refActivating ? '...' : t.profilePage.referralActivate}
            </button>
          </div>
        )}
      </div>

      {/* ─── Selected Exams (student only) ─── */}
      {isStudent && (<>
      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
        <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>{t.profilePage.examPreferences}</h3>
        <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
          {exams.map(exam => {
            const isSelected = selectedExams.includes(exam.code);
            return (
              <button
                key={exam.code}
                onClick={() => dispatch(toggleExamSelection(exam.code))}
                style={{
                  padding: '0.5rem 1rem', borderRadius: '999px', border: '2px solid',
                  borderColor: isSelected ? 'var(--primary-color)' : 'var(--border-color)',
                  background: isSelected ? 'rgba(79,70,229,0.1)' : 'transparent',
                  color: isSelected ? 'var(--primary-color)' : 'var(--text-secondary)',
                  fontWeight: 600, cursor: 'pointer', transition: 'all 0.2s', fontSize: '0.9rem',
                }}
              >
                {isSelected ? '✓ ' : ''}{exam.code}
              </button>
            );
          })}
        </div>
        {selectedExams.length === 0 && (
          <p style={{ color: 'var(--warning-color)', fontSize: '0.85rem', marginTop: '0.5rem' }}>
            {t.profilePage.selectAtLeastOne}
          </p>
        )}
      </div>

      {/* ─── Section Preferences ─── */}
      {allSections.length > 0 && (
        <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
          <h3 style={{ margin: '0 0 0.75rem', fontSize: '1rem' }}>{t.profilePage.sectionPreferences}</h3>
          {selectedExams.map(examCode => {
            const examSections = allSections.filter(s => s.examTypeCode === examCode);
            if (examSections.length === 0) return null;
            return (
              <div key={examCode} style={{ marginBottom: '0.75rem' }}>
                <p style={{ margin: '0 0 0.5rem', fontSize: '0.85rem', fontWeight: 600, color: 'var(--text-secondary)' }}>{examCode}</p>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.4rem' }}>
                  {examSections.map(section => {
                    const checked = selectedSectionIds.includes(section.id);
                    return (
                      <label
                        key={section.id}
                        style={{
                          display: 'flex', alignItems: 'center', gap: '0.6rem', padding: '0.5rem 0.75rem',
                          borderRadius: '8px', border: '2px solid',
                          borderColor: checked ? 'var(--primary-color)' : 'var(--border-color)',
                          background: checked ? 'rgba(79,70,229,0.08)' : 'transparent',
                          cursor: 'pointer', transition: 'all 0.2s', fontSize: '0.9rem',
                        }}
                      >
                        <input
                          type="checkbox"
                          checked={checked}
                          onChange={() => handleToggleSection(section.id)}
                          style={{ accentColor: 'var(--primary-color)', width: '16px', height: '16px' }}
                        />
                        <span style={{ color: checked ? 'var(--text-primary)' : 'var(--text-secondary)', fontWeight: checked ? 600 : 400 }}>
                          {section.name}
                        </span>
                      </label>
                    );
                  })}
                </div>
              </div>
            );
          })}
        </div>
      )}
      </>)}

      {/* ─── Change Password ─── */}
      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
        <h3 style={{ margin: '0 0 1rem', fontSize: '1rem' }}>{t.profilePage.changePassword}</h3>
        {pwdError && <div style={{ color: 'var(--error-color)', marginBottom: '0.75rem', padding: '0.5rem 0.75rem', background: 'var(--error-bg)', borderRadius: '6px', fontSize: '0.85rem' }}>{pwdError}</div>}
        {pwdSuccess && <div style={{ color: 'var(--success-color)', marginBottom: '0.75rem', padding: '0.5rem 0.75rem', background: 'rgba(16,185,129,0.08)', borderRadius: '6px', fontSize: '0.85rem' }}>✓ {pwdSuccess}</div>}
        <form onSubmit={handleChangePassword} style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          <div>
            <label className="form-label">{t.profilePage.currentPassword}</label>
            <input type="password" className="form-input" value={currentPassword} onChange={e => setCurrentPassword(e.target.value)} style={{ width: '100%' }} />
          </div>
          <div>
            <label className="form-label">{t.profilePage.newPassword}</label>
            <input type="password" className="form-input" value={newPassword} onChange={e => setNewPassword(e.target.value)} style={{ width: '100%' }} />
          </div>
          <div>
            <label className="form-label">{t.profilePage.confirmNewPassword}</label>
            <input type="password" className="form-input" value={confirmPassword} onChange={e => setConfirmPassword(e.target.value)} style={{ width: '100%' }} />
          </div>
          <button type="submit" className="btn btn-primary" disabled={pwdLoading || !currentPassword || !newPassword || !confirmPassword} style={{ alignSelf: 'flex-start', fontSize: '0.9rem' }}>
            {pwdLoading ? '...' : t.profilePage.changePassword}
          </button>
        </form>
      </div>

      {/* ─── Change Email ─── */}
      <div className="card" style={{ padding: '1.5rem', marginBottom: '1.25rem' }}>
        <h3 style={{ margin: '0 0 1rem', fontSize: '1rem' }}>{t.profilePage.newEmail}</h3>
        {emailError && <div style={{ color: 'var(--error-color)', marginBottom: '0.75rem', padding: '0.5rem 0.75rem', background: 'var(--error-bg)', borderRadius: '6px', fontSize: '0.85rem' }}>{emailError}</div>}
        {emailSuccess && <div style={{ color: 'var(--success-color)', marginBottom: '0.75rem', padding: '0.5rem 0.75rem', background: 'rgba(16,185,129,0.08)', borderRadius: '6px', fontSize: '0.85rem' }}>✓ {emailSuccess}</div>}
        <form onSubmit={handleChangeEmail} style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem' }}>
          <div>
            <label className="form-label">{t.profilePage.newEmail}</label>
            <input type="email" className="form-input" value={newEmail} onChange={e => setNewEmail(e.target.value)} placeholder="new@email.com" style={{ width: '100%' }} />
          </div>
          <div>
            <label className="form-label">{t.profilePage.enterPassword}</label>
            <input type="password" className="form-input" value={emailPassword} onChange={e => setEmailPassword(e.target.value)} style={{ width: '100%' }} />
          </div>
          <button type="submit" className="btn btn-primary" disabled={emailLoading || !newEmail || !emailPassword} style={{ alignSelf: 'flex-start', fontSize: '0.9rem' }}>
            {emailLoading ? '...' : t.common.save}
          </button>
        </form>
      </div>

    </div>
    <PricingModal isOpen={showPricing} onClose={() => setShowPricing(false)} />
    </>
  );
}

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <div style={{ color: 'var(--text-secondary)', fontSize: '0.75rem', textTransform: 'uppercase', letterSpacing: '0.04em' }}>{label}</div>
      <div style={{ fontWeight: 500 }}>{value}</div>
    </div>
  );
}

export default ProfilePage;
