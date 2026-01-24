export const createBoxArtUrl = (
  gameId: string,
  width: number = 285,
  height: number = 380
): string => {
  return `https://static-cdn.jtvnw.net/ttv-boxart/${gameId}-${width}x${height}.jpg`;
};

