import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  ComposableMap,
  Geographies,
  Geography,
  Marker,
  ZoomableGroup,
} from 'react-simple-maps';
import type { AdvisorUniversity } from '../advisorConfig';

const GEO_URL = '/countries-110m.json';

type Props = {
  universities: AdvisorUniversity[];
  /** IDs of universities matching current search. Empty set = show all equally. */
  highlightedIds: Set<string>;
  onMarkerClick?: (id: string) => void;
};

export default function AdvisorWorldMap({
  universities,
  highlightedIds,
  onMarkerClick,
}: Props) {
  const navigate = useNavigate();
  const [hoveredId, setHoveredId] = useState<string | null>(null);
  const [pinnedId, setPinnedId] = useState<string | null>(null);

  const withCoords = universities.filter(
    (u) => u.lat != null && u.lng != null,
  );

  // Pinned takes priority over hover for the tooltip
  const activeId = pinnedId ?? hoveredId;
  const activeUni = activeId ? withCoords.find((u) => u.id === activeId) : null;
  const hasHighlights = highlightedIds.size > 0;

  return (
    <div
      style={{
        position: 'relative',
        background: 'linear-gradient(180deg, #07111f 0%, #0d1f3a 100%)',
        borderRadius: 16,
        overflow: 'hidden',
        marginBottom: '2rem',
        border: '1px solid var(--border-color)',
      }}
    >
      {/* Title overlay */}
      <div
        style={{
          position: 'absolute',
          top: '0.85rem',
          left: '1.25rem',
          fontSize: '0.8rem',
          color: '#4a6fa5',
          fontWeight: 600,
          letterSpacing: '0.07em',
          textTransform: 'uppercase',
          zIndex: 2,
          pointerEvents: 'none',
        }}
      >
        {withCoords.length} вузов на карте
      </div>

      {/* Hint */}
      <div
        style={{
          position: 'absolute',
          top: '0.85rem',
          right: '1.25rem',
          fontSize: '0.75rem',
          color: '#2d4a6a',
          zIndex: 2,
          pointerEvents: 'none',
        }}
      >
        Скролл — масштаб · Перетащить — панорама
      </div>

      <ComposableMap
        projection="geoNaturalEarth1"
        projectionConfig={{ scale: 153, center: [0, 15] }}
        style={{ width: '100%', height: 380, display: 'block' }}
      >
        <ZoomableGroup zoom={1} minZoom={0.6} maxZoom={8}>
          <Geographies geography={GEO_URL}>
            {({ geographies }) =>
              geographies.map((geo) => (
                <Geography
                  key={geo.rsmKey}
                  geography={geo}
                  fill="#162236"
                  stroke="#1e3554"
                  strokeWidth={0.4}
                  style={{
                    default: { outline: 'none' },
                    hover: { outline: 'none', fill: '#1e3050' },
                    pressed: { outline: 'none' },
                  }}
                />
              ))
            }
          </Geographies>

          {withCoords.map((uni) => {
            const isHighlighted =
              !hasHighlights || highlightedIds.has(uni.id);
            const isHovered = hoveredId === uni.id;
            const isPinned = pinnedId === uni.id;

            const fillColor = isPinned
              ? '#f59e0b'
              : isHovered
                ? '#818cf8'
                : isHighlighted
                  ? '#6366f1'
                  : '#253555';
            const strokeColor = isPinned
              ? '#fcd34d'
              : isHovered
                ? '#c7d2fe'
                : isHighlighted
                  ? '#a5b4fc'
                  : '#3b526e';
            const radius = isPinned ? 11 : isHovered ? 10 : isHighlighted ? 7 : 4;

            return (
              <Marker
                key={uni.id}
                coordinates={[uni.lng!, uni.lat!]}
                onMouseEnter={() => setHoveredId(uni.id)}
                onMouseLeave={() => setHoveredId(null)}
                onClick={() => {
                  if (pinnedId === uni.id) {
                    setPinnedId(null);
                  } else {
                    setPinnedId(uni.id);
                    onMarkerClick?.(uni.id);
                  }
                }}
              >
                {/* Pulse ring for highlighted */}
                {isHighlighted && !isHovered && !isPinned && hasHighlights && (
                  <circle
                    r={13}
                    fill="none"
                    stroke="#6366f1"
                    strokeWidth={1}
                    opacity={0.35}
                  />
                )}
                {/* Pulse ring for pinned */}
                {isPinned && (
                  <circle
                    r={16}
                    fill="none"
                    stroke="#f59e0b"
                    strokeWidth={1.5}
                    opacity={0.5}
                  />
                )}
                <circle
                  r={radius}
                  fill={fillColor}
                  stroke={strokeColor}
                  strokeWidth={isHovered || isPinned ? 2.5 : 1.5}
                  style={{ cursor: 'pointer', transition: 'r 0.15s, fill 0.15s' }}
                />
                {/* Label shown on hover or pinned */}
                {(isHovered || isPinned) && (
                  <text
                    y={-18}
                    textAnchor="middle"
                    style={{
                      fontSize: '10px',
                      fill: isPinned ? '#fcd34d' : '#e2e8f0',
                      fontWeight: 600,
                      pointerEvents: 'none',
                      textShadow: '0 1px 3px #000',
                    }}
                  >
                    {uni.name}
                  </text>
                )}
              </Marker>
            );
          })}
        </ZoomableGroup>
      </ComposableMap>

      {/* Tooltip / Pinned card at bottom of map */}
      {activeUni && (
        <div
          style={{
            position: 'absolute',
            bottom: '1rem',
            left: '50%',
            transform: 'translateX(-50%)',
            background: pinnedId ? 'rgba(7, 17, 31, 0.97)' : 'rgba(7, 17, 31, 0.92)',
            border: `1px solid ${pinnedId ? '#f59e0b' : '#2d4a6a'}`,
            borderRadius: 10,
            padding: '0.75rem 1.25rem',
            minWidth: 260,
            maxWidth: 360,
            pointerEvents: pinnedId ? 'auto' : 'none',
            backdropFilter: 'blur(8px)',
            zIndex: 10,
          }}
        >
          {/* Close button when pinned */}
          {pinnedId && (
            <button
              onClick={() => setPinnedId(null)}
              style={{
                position: 'absolute',
                top: '0.4rem',
                right: '0.5rem',
                background: 'none',
                border: 'none',
                color: '#64748b',
                cursor: 'pointer',
                fontSize: '1rem',
                lineHeight: 1,
                padding: '0.1rem 0.3rem',
              }}
            >
              ✕
            </button>
          )}
          <div style={{ fontWeight: 700, color: '#e2e8f0', fontSize: '0.95rem', paddingRight: pinnedId ? '1.5rem' : 0 }}>
            {activeUni.name}
          </div>
          <div style={{ fontSize: '0.82rem', color: '#64748b', marginTop: '0.2rem' }}>
            {activeUni.city}, {activeUni.country}
          </div>
          <div style={{ fontSize: '0.82rem', color: '#94a3b8', marginTop: '0.15rem' }}>
            {activeUni.exam}
            {activeUni.minScore ? ` · мин. балл ${activeUni.minScore}` : ' · балл не указан'}
            {' · '}
            {activeUni.language}
          </div>
          {activeUni.specialties.length > 0 && (
            <div style={{ fontSize: '0.78rem', color: '#818cf8', marginTop: '0.4rem' }}>
              {activeUni.specialties.slice(0, 4).join(' · ')}
              {activeUni.specialties.length > 4
                ? ` +${activeUni.specialties.length - 4}`
                : ''}
            </div>
          )}
          {/* Open detail page button — only when pinned */}
          {pinnedId && (
            <button
              onClick={() => navigate(`/advisor/university/${activeUni.id}`)}
              style={{
                marginTop: '0.75rem',
                width: '100%',
                padding: '0.45rem 0.75rem',
                background: '#f59e0b',
                color: '#1a1a1a',
                border: 'none',
                borderRadius: '6px',
                cursor: 'pointer',
                fontWeight: 700,
                fontSize: '0.82rem',
                letterSpacing: '0.02em',
              }}
            >
              Открыть страницу вуза →
            </button>
          )}
        </div>
      )}

      {/* Legend */}
      <div
        style={{
          position: 'absolute',
          bottom: '0.8rem',
          right: '1rem',
          display: 'flex',
          gap: '1rem',
          fontSize: '0.73rem',
          color: '#3b526e',
          zIndex: 2,
        }}
      >
        {hasHighlights && (
          <>
            <span>
              <span
                style={{
                  display: 'inline-block',
                  width: 10,
                  height: 10,
                  borderRadius: '50%',
                  background: '#6366f1',
                  marginRight: 4,
                  verticalAlign: 'middle',
                }}
              />
              Подходит
            </span>
            <span>
              <span
                style={{
                  display: 'inline-block',
                  width: 8,
                  height: 8,
                  borderRadius: '50%',
                  background: '#253555',
                  marginRight: 4,
                  verticalAlign: 'middle',
                }}
              />
              Не подходит
            </span>
          </>
        )}
        {!hasHighlights && (
          <span>
            <span
              style={{
                display: 'inline-block',
                width: 10,
                height: 10,
                borderRadius: '50%',
                background: '#6366f1',
                marginRight: 4,
                verticalAlign: 'middle',
              }}
            />
            Вузы платформы
          </span>
        )}
      </div>
    </div>
  );
}
