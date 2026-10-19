import { parseAdultDate } from '../src/lib/dateOfBirth';
const now = new Date('2026-10-19T12:00:00Z');
it('accepts an eighteenth birthday today', () => expect(parseAdultDate('2008-10-19', now)).not.toBeNull());
it('rejects a person whose eighteenth birthday is tomorrow', () => expect(parseAdultDate('2008-10-20', now)).toBeNull());
it('rejects dates that JavaScript would normalize', () => expect(parseAdultDate('2000-02-30', now)).toBeNull());
