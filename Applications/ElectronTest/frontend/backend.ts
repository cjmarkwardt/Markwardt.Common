export interface Backend<T> {
  setWidth(width: number): void;
  setHeight(height: number): void;
  setDevTools(shown: boolean): void;
  setDevToolsShortcut(enabled: boolean): void;
  setFullscreen(fullscreen: boolean): void;
  setFullscreenShortcut(enabled: boolean): void;
  setMaximized(maximized: boolean): void;
  setMinimized(minimized: boolean): void;

  /** Fire-and-forget: send a message to the backend with no expected response. */
  send(message: T): void;

  /** Request/response: send a message and await a single reply from the backend. */
  request(message: T): Promise<T>;
  
  /**
   * Push subscription: register a handler for all unsolicited messages arriving
   * from the backend. Returns a cleanup function — call it when the handler is
   * no longer needed (e.g. in a useEffect cleanup or component unmount).
   */
  listen(handler: (message: T, respond?: (message: T) => void) => void): () => void;
}