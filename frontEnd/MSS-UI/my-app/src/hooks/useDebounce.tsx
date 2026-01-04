import { useEffect, useState } from "react";

export function useDebounce<T>(value: T, delay = 300, isSkip: boolean) {
  const [debounced, setDebounced] = useState(value);
  console.log("skip debounce is  :" + isSkip);

  useEffect(() => {
    if (isSkip) {
      //setDebounced(value);
      console.log("debounce is skipped");
      return;
    }

    const timer = setTimeout(() => setDebounced(value), delay);
    return () => clearTimeout(timer);
  }, [value]);

  return debounced;
}
