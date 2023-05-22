// host name
let referrer = "";

///////////////////////////////////////
// Please select environment

export const env = "prod";
// export const env = "dev";
// export const env = "local";

// API URL for CM-Team

const hostname = window.location.hostname;
console.log("hostname : ", hostname);

if (hostname === "nan5dfc1web01.corpadds.com") {
  referrer = "http://nan5dfc1web01.corpadds.com:8084/cmtools";
} else {
  referrer = "http://localhost:58994";
}

export const clientListAPI = referrer + "/api/toolbox/clients";
export const categoryAPI = referrer + "/api/buildversion/category/list/1";
export const adminClientAPI = referrer + "/api/admclients/";
export const restoreSettingAPI = referrer + "/api/RestoreRequestSetting";
export const userAPI = referrer + "/api/user";

export const namespaceAPI = referrer + "/api/namespace?qr=";
export const restoreListAPI = referrer + "/api/restorelist";
export const restoreBackupPOSTAPI = referrer + "/api/restorebackup";

export const fileCheckAPI = referrer + "/api/filecheck?filePath=";
export const fileServerCheckAPI = referrer + "/api/FileServerCheck";

export const serverAPI = referrer + "/api/server";
export const settingAPI = referrer + "/api/setting";

export { referrer };

// API URL for Comapre Pay Tool

let referrer_cpt = "";

if (hostname === "nan5dfc1web01.corpadds.com") {
  referrer_cpt = "http://nan5dfc1web01.corpadds.com:8084/server";
} else {
  referrer_cpt = "http://localhost:5000";
}

export const inputAPI = referrer_cpt + "/api/input/";

export const analyzeAPI = referrer_cpt + "/api/analyzepsr/";

export const analyzeBRRAPI = referrer_cpt + "/api/analyzebrr/";

export const analyzeJSRAPI = referrer_cpt + "/api/analyzejsr/";

export const analyzeSCRAPI = referrer_cpt + "/api/analyzescr/";

export const analyzeAEAPI = referrer_cpt + "/api/analyzeae/";

export const analyzeEAPI = referrer_cpt + "/api/analyzee/";

export const CPTtracerAPI = referrer_cpt + "/api/tracer/";

export { referrer_cpt };

// API URL for Tracer API - NOTE: After being able to call Tracer API, configure Tracer API url

let referrer_tracer = "";

if (hostname === "nan5dfc1web01.corpadds.com") {
  referrer_tracer = "http://localhost:51000";
} else {
  referrer_tracer = "http://localhost:51000";
}

export const tracerAPI =
  referrer_tracer + "/u/GPNcpL9ZGkKdTmlbSDMpgQ/Timesheet/Tracer/Tracer";

export { referrer_tracer };
