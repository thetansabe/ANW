import { LogLevel } from "@aspnet/signalr";

// This file can be replaced during build by using the `fileReplacements` array.
// `ng build ---prod` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,
  HOST:"http://localhost:6715",
  MEDIA_URL:"http://localhost:6715/_media/",
  RSRC_URL:"http://localhost:6715/resources/",
  ClaimRole:"http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
  secret_key:"jb4n7huyPibrEcRhjTDkgKtQgZRwUXZv.r92b|A#xS+H9^1kW0-WL",
  LogLevel: LogLevel.Debug
};

/*
 * In development mode, to ignore zone related error stack frames such as
 * `zone.run`, `zoneDelegate.invokeTask` for easier debugging, you can
 * import the following file, but please comment it out in production mode
 * because it will have performance impact when throw error
 */
// import 'zone.js/dist/zone-error';  // Included with Angular CLI.
