import { RefObject, useEffect } from "react";

export const useOutsideClick = (
  ref: RefObject<HTMLElement | null>,
  callback: (event: Event) => void
) => {
  useEffect(() => {
    const listener = (event: Event) => {
      // Ignore clicks that originate from inside the referenced element
      const targetNode = event.target as Node | null;
      const composedPath =
        typeof (event as unknown as { composedPath?: () => EventTarget[] }).composedPath ===
        "function"
          ? (event as unknown as { composedPath: () => EventTarget[] }).composedPath()
          : undefined;

      if (composedPath && ref.current && composedPath.includes(ref.current)) {
        return;
      }

      if (!ref.current || (targetNode && ref.current.contains(targetNode))) {
        return;
      }
      callback(event);
    };

    document.addEventListener("mousedown", listener);
    document.addEventListener("touchstart", listener);

    return () => {
      document.removeEventListener("mousedown", listener);
      document.removeEventListener("touchstart", listener);
    };
  }, [ref, callback]);
};
