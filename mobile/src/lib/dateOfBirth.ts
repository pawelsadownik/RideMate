export function parseAdultDate(value: string, now = new Date()): Date | null {
  if (!/^\d{4}-\d{2}-\d{2}$/.test(value)) return null;
  const date = new Date(value + 'T00:00:00Z');
  if (!Number.isFinite(date.getTime()) || date.toISOString().slice(0, 10) !== value) return null;
  const cutoff = new Date(Date.UTC(now.getFullYear() - 18, now.getMonth(), now.getDate()));
  return date <= cutoff && date.getUTCFullYear() >= 1900 ? date : null;
}
