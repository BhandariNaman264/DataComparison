import React, { useEffect, useState } from "react";
import axios from "axios";
import {
  Box,
  Grid,
  Button,
  Container,
  makeStyles,
  withStyles,
} from "@material-ui/core";
import Page from "src/components/Page";
import { ClientCard } from "./ClientCard/index";
import VersionsCard from "./VersionsCard/VersionsCard";
import TargetJobCard from "./JobCard/TargetJobCard";
import ConfriguationsCard from "./ConfriguationsCard/ConfriguationsCard";
import RunComparisonCard from "./RunComparisonCard/RunComparisonCard";
import {
  clientListAPI,
  categoryAPI,
  adminClientAPI,
} from "src/components/APIBase/BaseURL";
import "react-loader-spinner/dist/loader/css/react-spinner-loader.css";

const useStyles = makeStyles((theme) => ({
  root: {
    backgroundColor: theme.palette.background.white,
    marginBottom: 35,
    paddingBottom: theme.spacing(3),
    paddingTop: theme.spacing(3),
  },
  subheader: {
    margin: "5px",
    fontSize: "16px",
  },
  icon: {
    position: "relative",
    top: 4,
  },
}));

const ColorButton = withStyles((theme) => ({
  root: {
    color: theme.palette.getContrastText("#f44336"),
    backgroundColor: "#f44336",
    "&:hover": {
      backgroundColor: "#c51162",
    },
  },
}))(Button);

const ComparisonForm = ({ userInfo }) => {
  const classes = useStyles();
  const [categoryList, setCategoryList] = useState([]);
  const [admClientList, setAdmClientList] = useState([]);

  const [list, setList] = useState([]);
  const [Job, setJob] = useState({
    jobName: "",
    id: 0,
  });

  const [ForceCompare, setForceCompare] = useState({
    booleanValue: "",
    val: -1,
    bool: null,
  });

  const [RunTask1, setRunTask1] = useState({
    booleanValue: "",
    val: -1,
    bool: null,
  });
  const [RunTask2, setRunTask2] = useState({
    booleanValue: "",
    val: -1,
    bool: null,
  });
  const [Org, setOrg] = useState("");

  const [PolicyName, setPolicyName] = useState("");

  const [TaskName, setTaskName] = useState("");

  const [PGCalendar, setPGCalendar] = useState("");

  const [ModeExport, setModeExport] = useState("");

  const [ExportFileName, setExportFileName] = useState("");

  const [TransmitMock, setTransmitMock] = useState({
    booleanValue: "",
    val: -1,
    bool: null,
  });

  const [Start, setStart] = useState(null);
  const [End, setEnd] = useState(null);

  const [DateRelative, setDateRelative] = useState(null);

  const [clientList, setClientList] = useState([
    {
      adminDb: "",
      clientId: "",
      clientName: "",
      adminDbSrv: "",
      clientDb: "",
      databaseVersion: "",
      dbSize: 0,
      site: "",
    },
  ]);
  const [ClientInfo, setClientInfo] = useState({
    id: 0,
    envName: "",
    category: "",
    status: 0,
    filePath: "",
    dbSize: 0,
    dbSize2: 0,
    site: "",
    clientDb: "",
    clientId: -1,
    clientName: "",
    adminDb: "",
    adminDbSrv: "",
    namespace: null,
    namespace2: null,
  });

  const [valid, setValid] = useState(0); // 0 = not checked, 1 = valid, -1 = not valid
  const [subOpen, setSubOpen] = useState(false);

  const reset = () => {
    setJob((prev) => {
      let update = { ...prev };
      update.jobName = "";
      update.id = 0;
      return update;
    });

    setRunTask1((prev) => {
      let update = { ...prev };
      update.booleanValue = "";
      update.val = -1;
      update.bool = null;
      return update;
    });

    setRunTask2((prev) => {
      let update = { ...prev };
      update.booleanValue = "";
      update.val = -1;
      update.bool = null;
      return update;
    });

    setForceCompare((prev) => {
      let update = { ...prev };
      update.booleanValue = "";
      update.val = -1;
      update.bool = null;
      return update;
    });

    setTaskName("");
    setOrg("");
    setPolicyName("");

    setPGCalendar("");
    setExportFileName("");
    setModeExport("");
    setTransmitMock((prev) => {
      let update = { ...prev };
      update.booleanValue = "";
      update.val = -1;
      update.bool = null;
      return update;
    });

    setStart(null);
    setEnd(null);
    setDateRelative(null);

    setClientInfo((prev) => {
      let update = { ...prev };
      update.id = 0;
      update.envName = "";
      update.filePath = "";
      update.category = "";
      update.status = 0;
      update.clientId = -1;
      update.clientName = "";
      update.clientDb = "";
      update.site = "";
      update.dbSize = 0;
      update.dbSize2 = 0;
      update.adminDb = "";
      update.adminDbSrv = "";
      update.namespace = null;
      update.namespace2 = null;
      return update;
    });

    setValid(0);
  };

  const jobchangereset = () => {
    setRunTask1((prev) => {
      let update = { ...prev };
      update.booleanValue = "";
      update.val = -1;
      update.bool = null;
      return update;
    });

    setRunTask2((prev) => {
      let update = { ...prev };
      update.booleanValue = "";
      update.val = -1;
      update.bool = null;
      return update;
    });

    setForceCompare((prev) => {
      let update = { ...prev };
      update.booleanValue = "";
      update.val = -1;
      update.bool = null;
      return update;
    });

    setTaskName("");
    setOrg("");
    setPolicyName("");

    setPGCalendar("");
    setExportFileName("");
    setModeExport("");
    setTransmitMock((prev) => {
      let update = { ...prev };
      update.booleanValue = "";
      update.val = -1;
      update.bool = null;
      return update;
    });

    setStart(null);
    setEnd(null);
    setDateRelative(null);

    setClientInfo((prev) => {
      let update = { ...prev };
      update.id = 0;
      update.envName = "";
      update.filePath = "";
      update.category = "";
      update.status = 0;
      update.clientId = -1;
      update.clientName = "";
      update.clientDb = "";
      update.site = "";
      update.dbSize = 0;
      update.dbSize2 = 0;
      update.adminDb = "";
      update.adminDbSrv = "";
      update.namespace = null;
      update.namespace2 = null;
      return update;
    });

    setValid(0);
  };

  useEffect(() => {
    let clientApi = async () => {
      let url = clientListAPI;
      await axios({
        method: "GET",
        url,
      })
        .then(function (response) {
          setList(response.data);
        })
        .catch(function (error) {
          console.log(error);
        });
    };
    clientApi();
  }, []);

  useEffect(() => {
    let catApi = async () => {
      let url = categoryAPI;
      await axios({
        method: "GET",
        url,
        "Access-Control-Allow-Credentials": true,
      })
        .then(function (response) {
          setCategoryList(response.data);
        })
        .catch(function (error) {
          console.log(error);
        });
    };
    catApi();
  }, []);

  useEffect(() => {
    let admClientApi = async () => {
      let url = adminClientAPI + "ALL";
      await axios({
        method: "GET",
        url,
      })
        .then(function (response) {
          setAdmClientList(response.data);
        })
        .catch(function (error) {
          console.log(error);
        });
    };
    admClientApi();
  }, []);

  useEffect(() => {
    class client {
      constructor(
        clientId,
        clientName,
        adminDbSrv,
        namespace,
        databaseVersion,
        dbSize,
        site
      ) {
        this.adminDb = databaseVersion;
        this.clientId = parseInt(clientId);
        this.clientName = clientName;
        this.adminDbSrv = adminDbSrv;
        this.clientDb = namespace;
        this.dbSize = dbSize;
        this.site = site === "Prod" ? "Production" : site;
      }
    }
    let tmp = [];
    list.forEach(function (item) {
      if (item.clientName !== "") {
        var obj = new client(
          item.clientID,
          item.clientName,
          item.clientServer,
          item.namespace,
          item.databaseVersion,
          item.prodBackupSizeMB,
          item.site
        );
        tmp.push(obj);
      }
    });
    setClientList(
      tmp.sort((a, b) => parseInt(a.clientId) - parseInt(b.clientId))
    );
  }, [list]);

  useEffect(() => {
    let backupClientApi = async () => {
      // for backup file path source environment, will assume that the admin DB client names take precedence, and will just add on additional toolbox client names
      var seen = {};
      var out = [];
      var j = 0;
      // since we are backing up from a backup file, we don't need anything more than the client name and the client ID
      // we can iterate through the list and only grab unique client IDs and put these unique clients into a set
      // use the set as a look up to check if we have already seen this client already
      // first add the Admin DB clients into the set as well as the resulting list
      admClientList.forEach(function (item) {
        item.site = "DF R&D";
        item.clientDb = "";
        seen[item.clientId] = 1;
        out[j++] = item;
      });
      // now have to parse through the clientList and only grab clients with client ID's that are already not in the set to maintain uniqueness
      for (var i = 0; i < clientList.length; i++) {
        var id = clientList[i].clientId;
        // since out list is composed of objects (clients), we only need to check the client ID
        if (seen[id] !== 1) {
          seen[id] = 1;
          // if  we haven't seen the ID before, we can add the object (client) into our list
          out[j++] = clientList[i];
        }
      }
      // once you have your unique client ID list, we can sort by client ID and set it afor the options dropdown
      // setBackupList(
      //   out.sort((a, b) => parseInt(a.clientId) - parseInt(b.clientId))
      // );
    };
    backupClientApi();
  }, [admClientList, clientList]);

  useEffect(() => {
    setSubOpen(false);
  }, [ClientInfo]);

  return (
    <Page className={classes.root} title="Comparison Page">
      <Container maxWidth={false}>
        <Grid container spacing={3}>
          <Grid item lg={12} md={12} xl={12} xs={12}>
            <TargetJobCard
              Job={Job}
              setJob={setJob}
              setSubOpen={setSubOpen}
              jobchangereset={jobchangereset}
            />
          </Grid>
          {Job.id === 1 ||
          Job.id === 2 ||
          Job.id === 3 ||
          Job.id === 4 ||
          Job.id === 5 ||
          Job.id === 6 ? (
            <Grid item lg={6} md={6} xl={6} xs={6}>
              <ClientCard
                ClientInfo={ClientInfo}
                setClientInfo={setClientInfo}
                valid={valid}
                setValid={setValid}
                categoryList={categoryList}
                clientList={clientList}
                setSubOpen={setSubOpen}
              />
            </Grid>
          ) : (
            <></>
          )}
          {Job.id === 1 ||
          Job.id === 2 ||
          Job.id === 3 ||
          Job.id === 4 ||
          Job.id === 5 ||
          Job.id === 6 ? (
            <Grid item lg={6} md={6} xl={6} xs={6}>
              <VersionsCard
                ClientInfo={ClientInfo}
                setClientInfo={setClientInfo}
                setSubOpen={setSubOpen}
              />
            </Grid>
          ) : (
            <></>
          )}
          {Job.id === 1 ||
          Job.id === 2 ||
          Job.id === 3 ||
          Job.id === 4 ||
          Job.id === 5 ||
          Job.id === 6 ? (
            <Grid item lg={12} md={12} xl={12} xs={12}>
              <ConfriguationsCard
                ForceCompare={ForceCompare}
                setForceCompare={setForceCompare}
                RunTask1={RunTask1}
                setRunTask1={setRunTask1}
                RunTask2={RunTask2}
                setRunTask2={setRunTask2}
                Org={Org}
                setOrg={setOrg}
                TaskName={TaskName}
                setTaskName={setTaskName}
                Start={Start}
                setStart={setStart}
                End={End}
                setEnd={setEnd}
                setSubOpen={setSubOpen}
                DateRelative={DateRelative}
                setDateRelative={setDateRelative}
                Job={Job}
                setJob={setJob}
                PolicyName={PolicyName}
                setPolicyName={setPolicyName}
                PGCalendar={PGCalendar}
                setPGCalendar={setPGCalendar}
                TransmitMock={TransmitMock}
                setTransmitMock={setTransmitMock}
                ModeExport={ModeExport}
                setModeExport={setModeExport}
                ExportFileName={ExportFileName}
                setExportFileName={setExportFileName}
              />
            </Grid>
          ) : (
            <></>
          )}

          {Job.id === 1 ||
          Job.id === 2 ||
          Job.id === 3 ||
          Job.id === 4 ||
          Job.id === 5 ||
          Job.id === 6 ? (
            <Grid item lg={12} md={12} xl={12} xs={12}>
              <RunComparisonCard
                ClientInfo={ClientInfo}
                Job={Job}
                ForceCompare={ForceCompare}
                RunTask1={RunTask1}
                RunTask2={RunTask2}
                Org={Org}
                TaskName={TaskName}
                Start={Start}
                End={End}
                userInfo={userInfo}
                reset={reset}
                setSubOpen={setSubOpen}
                subOpen={subOpen}
                DateRelative={DateRelative}
                PolicyName={PolicyName}
                PGCalendar={PGCalendar}
                TransmitMock={TransmitMock}
                ModeExport={ModeExport}
                ExportFileName={ExportFileName}
              />
            </Grid>
          ) : (
            <></>
          )}

          {valid === -1 ? (
            <Grid item lg={6} md={6} xl={6} xs={12}>
              <Box mb={1}>
                <ColorButton
                  style={{ float: "left" }}
                  id="cancelButton"
                  variant="contained"
                  color="primary"
                  onClick={reset}
                >
                  Cancel Request
                </ColorButton>
              </Box>
            </Grid>
          ) : (
            <></>
          )}
        </Grid>
      </Container>
    </Page>
  );
};

export default ComparisonForm;
