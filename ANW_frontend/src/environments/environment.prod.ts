import { LogLevel } from "@aspnet/signalr";

export const environment = {
  production: true,
  HOST:"http://localhost:6715",
  MEDIA_URL:"http://localhost:6715/_media/",
  RSRC_URL:"http://localhost:6715/resources/",
  ClaimRole:"http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
  secret_key:"jb4n7huyPibrEcRhjTDkgKtQgZRwUXZv.r92b|A#xS+H9^1kW0-WL",
  LogLevel: LogLevel.None
};
