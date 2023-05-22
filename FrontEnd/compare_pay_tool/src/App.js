import React, { useEffect, useState } from "react";
import { HashRouter as Router, Route, Redirect } from "react-router-dom";
import { ThemeProvider } from "@material-ui/core";
import axios from "axios";
import "./App.css";
import theme from "./theme";
import GlobalStyles from "./components/GlobalStyles";
import ComparisonForm from "./views/ComparePayView";
import DashboardLayout from "src/layout";
import { env, userAPI } from "./components/APIBase/BaseURL"; // import { env, userAPI, referrer } from "./components/APIBase/BaseURL";
//import { userWhiteList } from './components/UserWhiteList';
import "./mixins/chartjs";
import MaintenanceRedirect from "./views/MaintenanceView";
import Home from "./views/HomeView/Home";
import ResultsPage from "./views/ResultsPageView/ResultsPage";
import ComparePayTool from "./views/AboutView/ComparePayTool";
import AnalyzePage from "./views/AnalyzeView/AnalyzePage";
import RulePage from "./views/RuleView/RulePage";

export default function App() {
  // Run user windows auth API here since App.js only renders once between navigation of tabs
  const [start, setStart] = useState(true);
  const [error, setError] = useState(false);
  const [userInfo, setUserInfo] = useState({
    displayName: "",
    distinguishedName: "",
    domain: "",
    givenName: "",
    isMemberOf: false,
    role: "",
    samAccountName: "",
    surname: "",
    tfsName: "",
    user: "",
    userPrincipalName: "",
  });

  useEffect(() => {
    let userApi = async () => {
      let url = userAPI;
      await axios({
        method: "GET",
        url,
        headers: { withCredentials: true },
      })
        .then(function (response) {
          setUserInfo(response.data);
          setStart(false);
        })
        .catch(function (error) {
          setError(true);
          console.log(error);
          setStart(false);
        });
    };

    if (env === "prod" || env === "dev") {
      userApi();
    } else {
      setUserInfo((prev) => {
        let update = { ...prev };
        update.displayName = "Bhandari, Naman";
        update.distinguishedName =
          "CN=Bhandari, Naman (P264),OU=Standard,OU=Users,OU=Root,DC=corpadds,DC=com";
        update.domain = "CORPADDS";
        update.givenName = "Naman";
        update.isMemberOf = false;
        update.role = "Intern-Developer";
        update.samAccountName = "P264";
        update.surname = "Bhandari";
        update.tfsName = "Bhandari, Naman &lt;CORPADDS\\P264&gt;";
        update.user = "Bhandari, Naman(p264)";
        update.userPrincipalName = "Naman.Bhandari@ceridian.com";
        update.server = "nan5dfc1sql07";
        update.database = "ComparePayTool";
        return update;
      });
      setStart(false);
    }
  }, []);
  return (
    <ThemeProvider theme={theme}>
      <GlobalStyles />
      <div className="App">
        {start ? (
          <Router></Router>
        ) : error ? (
          <Router>
            <Route exact path="/" component={() => <MaintenanceRedirect />} />
            <Route
              exact
              path="/comparison"
              component={() => <Redirect to="/" />}
            />
            <Route
              exact
              path="/comparepaytool"
              component={() => <Redirect to="/" />}
            />
            <Route exact path="/result" component={() => <Redirect to="/" />} />
            <Route
              exact
              path="/analyze/:id"
              component={() => <Redirect to="/" />}
            />
            <Route
              exact
              path="/rule/:id"
              component={() => <Redirect to="/" />}
            />
          </Router>
        ) : (
          <Router>
            <Route
              exact
              path="/"
              component={() => (
                <DashboardLayout userInfo={userInfo}>
                  <Home userInfo={userInfo} />
                </DashboardLayout>
              )}
            />
            <Route
              exact
              path="/comparison"
              component={() => (
                <DashboardLayout userInfo={userInfo}>
                  <ComparisonForm userInfo={userInfo} />
                </DashboardLayout>
              )}
            />
            <Route
              exact
              path="/comparepaytool"
              component={() => (
                <DashboardLayout userInfo={userInfo}>
                  <ComparePayTool />
                </DashboardLayout>
              )}
            />
            <Route
              exact
              path="/result"
              component={() => (
                <DashboardLayout userInfo={userInfo}>
                  <ResultsPage userInfo={userInfo} />
                </DashboardLayout>
              )}
            />
            <Route
              exact
              path="/analyze/:id"
              component={() => (
                <DashboardLayout userInfo={userInfo}>
                  <AnalyzePage />
                </DashboardLayout>
              )}
            />
            <Route
              exact
              path="/rule/:id"
              component={() => (
                <DashboardLayout userInfo={userInfo}>
                  <RulePage />
                </DashboardLayout>
              )}
            />
          </Router>
        )}
      </div>
    </ThemeProvider>
  );
}
