/**
 * @jest-environment jsdom
 */

// Mock localStorage
const localStorageMock = {
  getItem: jest.fn(),
  setItem: jest.fn(),
  removeItem: jest.fn(),
  clear: jest.fn(),
};

Object.defineProperty(global, 'localStorage', { value: localStorageMock });

// Import after mock is set up
import { draftStorage } from './useDraftStorage';

describe('useDraftStorage', () => {
  beforeEach(() => {
    localStorageMock.getItem.mockClear();
    localStorageMock.setItem.mockClear();
    localStorageMock.removeItem.mockClear();
    localStorageMock.clear.mockClear();
  });

  describe('save', () => {
    it('should save data to localStorage', () => {
      draftStorage.save('test-form', { name: 'test' });

      expect(localStorageMock.setItem).toHaveBeenCalled();
      const call = localStorageMock.setItem.mock.calls[0];
      expect(call[0]).toBe('nexterp_draft_test-form');
      const saved = JSON.parse(call[1]);
      expect(saved.data).toEqual({ name: 'test' });
      expect(saved.key).toBe('test-form');
    });

    it('should update index when saving', () => {
      localStorageMock.getItem.mockReturnValueOnce(null);
      draftStorage.save('test-form', { name: 'test' });

      expect(localStorageMock.setItem).toHaveBeenCalledTimes(2);
    });

    it('should set correct expiration time', () => {
      const ttl = 1000;
      draftStorage.save('test-form', { name: 'test' }, ttl);

      const call = localStorageMock.setItem.mock.calls[0];
      const saved = JSON.parse(call[1]);
      expect(saved.expiresAt - saved.savedAt).toBe(ttl);
    });
  });

  describe('load', () => {
    it('should return null when no draft exists', () => {
      localStorageMock.getItem.mockReturnValueOnce(null);

      const result = draftStorage.load('nonexistent');

      expect(result).toBeNull();
    });

    it('should return draft when exists and not expired', () => {
      const draft = {
        key: 'test-form',
        data: { name: 'test' },
        savedAt: Date.now(),
        expiresAt: Date.now() + 10000,
      };
      localStorageMock.getItem.mockReturnValueOnce(JSON.stringify(draft));

      const result = draftStorage.load('test-form');

      expect(result).toEqual(draft);
    });

    it('should return null and remove when expired', () => {
      const expiredDraft = {
        key: 'test-form',
        data: { name: 'test' },
        savedAt: Date.now() - 20000,
        expiresAt: Date.now() - 10000,
      };
      localStorageMock.getItem
        .mockReturnValueOnce(JSON.stringify(expiredDraft))
        .mockReturnValueOnce(JSON.stringify({ 'test-form': Date.now() - 20000 }));

      const result = draftStorage.load('test-form');

      expect(result).toBeNull();
      expect(localStorageMock.removeItem).toHaveBeenCalled();
    });
  });

  describe('remove', () => {
    it('should remove draft from localStorage', () => {
      localStorageMock.getItem.mockReturnValueOnce(
        JSON.stringify({ 'test-form': Date.now() })
      );

      draftStorage.remove('test-form');

      expect(localStorageMock.removeItem).toHaveBeenCalledWith(
        'nexterp_draft_test-form'
      );
    });
  });

  describe('exists', () => {
    it('should return true when draft exists', () => {
      const draft = {
        key: 'test-form',
        data: { name: 'test' },
        savedAt: Date.now(),
        expiresAt: Date.now() + 10000,
      };
      localStorageMock.getItem.mockReturnValueOnce(JSON.stringify(draft));

      const result = draftStorage.exists('test-form');

      expect(result).toBe(true);
    });

    it('should return false when no draft exists', () => {
      localStorageMock.getItem.mockReturnValueOnce(null);

      const result = draftStorage.exists('nonexistent');

      expect(result).toBe(false);
    });
  });

  describe('cleanup', () => {
    it('should remove expired drafts', () => {
      const index = {
        'expired-form': Date.now() - 10000,
        'valid-form': Date.now(),
      };
      const expiredDraft = {
        key: 'expired-form',
        data: {},
        savedAt: Date.now() - 10000,
        expiresAt: Date.now() - 5000,
      };
      localStorageMock.getItem
        .mockReturnValueOnce(JSON.stringify(index))
        .mockReturnValueOnce(JSON.stringify(expiredDraft));

      draftStorage.cleanup();

      expect(localStorageMock.removeItem).toHaveBeenCalledWith(
        'nexterp_draft_expired-form'
      );
    });
  });

  describe('clearAll', () => {
    it('should clear all drafts', () => {
      localStorageMock.getItem.mockReturnValueOnce(
        JSON.stringify({ 'form1': Date.now(), 'form2': Date.now() })
      );

      draftStorage.clearAll();

      expect(localStorageMock.removeItem).toHaveBeenCalledTimes(3);
      expect(localStorageMock.clear).toHaveBeenCalled();
    });
  });
});
