// 開発環境（ローカル）と本番環境で url 等を分けるために使用
export const isLocalhost = (): boolean => {
  return ["localhost", "127.0.0.1"].includes(window.location.hostname);
};