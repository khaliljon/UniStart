import { useMemo, useState } from 'react';
import { COUNTRIES, splitPhone, type Country } from '../utils/countries';

interface Props {
  value: string;
  onChange: (fullNumber: string) => void;
  id?: string;
  disabled?: boolean;
}

export default function PhoneField({ value, onChange, id, disabled }: Props) {
  const [country, setCountry] = useState<Country>(() => splitPhone(value).country);

  const national = useMemo(() => {
    const digits = (value ?? '').replace(/\D/g, '');
    return digits.startsWith(country.dial) ? digits.slice(country.dial.length) : digits;
  }, [value, country]);

  const emit = (c: Country, nat: string) => onChange(`+${c.dial}${nat.replace(/\D/g, '')}`);

  return (
    <div style={{ display: 'flex', gap: '0.4rem' }}>
      <select
        aria-label="Country code"
        value={country.code}
        disabled={disabled}
        onChange={(e) => {
          const c = COUNTRIES.find((x) => x.code === e.target.value) ?? country;
          setCountry(c);
          emit(c, national);
        }}
        className="form-input"
        style={{ width: 'auto', maxWidth: '9.5rem', flexShrink: 0 }}
      >
        {COUNTRIES.map((c) => (
          <option key={c.code} value={c.code}>{c.name} +{c.dial}</option>
        ))}
      </select>
      <input
        id={id}
        type="tel"
        inputMode="numeric"
        className="form-input"
        value={national}
        disabled={disabled}
        onChange={(e) => emit(country, e.target.value.replace(/\D/g, '').slice(0, country.max))}
        placeholder={'0'.repeat(country.max)}
        style={{ flex: 1, minWidth: 0 }}
      />
    </div>
  );
}
