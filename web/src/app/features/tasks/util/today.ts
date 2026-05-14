/**
 * Returns today's date in `YYYY-MM-DD` format. Used to set the `min` on date inputs so the
 * UI mirrors the domain rule that due dates cannot be in the past.
 */
export function todayIsoDate(now: Date = new Date()): string {
  const yyyy = now.getFullYear();
  const mm = String(now.getMonth() + 1).padStart(2, '0');
  const dd = String(now.getDate()).padStart(2, '0');
  return `${yyyy}-${mm}-${dd}`;
}
