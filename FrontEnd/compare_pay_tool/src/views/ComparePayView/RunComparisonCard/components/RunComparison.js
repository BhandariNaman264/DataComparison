import React from "react";
import {
  Button,
  Snackbar,
  FormControl,
  FormControlLabel,
  FormLabel,
  Box,
  makeStyles,
  Grid,
  Radio,
  RadioGroup,
} from "@material-ui/core";
import MuiAlert from "@material-ui/lab/Alert";
import { inputAPI } from "src/components/APIBase/BaseURL";
import "react-loader-spinner/dist/loader/css/react-spinner-loader.css";
import { useState } from "react";
import axios from "axios";

function Alert(props) {
  return <MuiAlert elevation={6} variant="filled" {...props} />;
}

const useStyles = makeStyles((theme) => ({
  formControl: {
    margin: theme.spacing(1),
    minWidth: 400,
    maxWidth: 400,
  },
  buttonProgress: {
    color: "#4caf50",
    position: "absolute",
    top: "50%",
    left: "50%",
    marginTop: -12,
    marginLeft: -12,
  },
}));

const ButtonCompare = ({
  ClientInfo,
  Job,
  ForceCompare,
  RunTask1,
  RunTask2,
  Org,
  TaskName,
  Start,
  End,
  userInfo,
  reset,
  setSubOpen,
  subOpen,
  DateRelative,
  PolicyName,
  PGCalendar,
  TransmitMock,
  ModeExport,
  ExportFileName,
}) => {
  const classes = useStyles();

  const [buttonType, setbuttonType] = useState("START");

  const [download, setdownload] = useState("false");
  const [open, setOpen] = useState(false);
  const [success, setSuccess] = useState(true);
  const [msg, setMsg] = useState(
    "Click RESET to Reset All Confriguations or Change any confriguation manually to run Comparison again with new confriguations or to Download Input File with new confriguations"
  );
  const [Reset, setReset] = useState(false);

  const handleClose = (event, reason) => {
    if (reason === "clickaway") {
      return;
    }
    setOpen(false);
  };

  const ComparisonRan = () => {
    try {
      setOpen(true);
      setSuccess(true);
      setSubOpen(true);
      setReset(true);
      setdownload("false");
      setbuttonType("START");
    } catch (error) {
      console.log(error);
      setMsg(
        "Oops! We encountered an error in Comparison. Sorry for the Inconvenience!"
      );
      setSuccess(false);
    }
  };

  // This piece of Code is to make Input PSR file using Input Parameters

  const createStringDate = (date) => {
    let d = date.toISOString();

    d = d.split("T")[0] + "T00:00:00";

    return d;
  };

  const createFileType = () => {
    let job = Job.file;

    job = "Input" + job + ".json";

    return job;
  };

  // This code is to make JSON File and download it in Client's Computer , and it uses the above function to format it properly

  const createNameSpace1Info = () => {
    let namespaceInfo =
      ClientInfo.namespace.controlDbServer +
      ";" +
      ClientInfo.namespace.controlDb;
    return namespaceInfo;
  };
  const createNameSpace2Info = () => {
    let namespaceInfo =
      ClientInfo.namespace2.controlDbServer +
      ";" +
      ClientInfo.namespace2.controlDb;
    return namespaceInfo;
  };

  const createInputPSRJSRBRR = async () => {
    let namespace1Info = createNameSpace1Info();
    let namespace2Info = createNameSpace2Info();

    const inputObject = {
      ControlDBNamespace: {
        [namespace1Info]: [ClientInfo.namespace.dbName],
        [namespace2Info]: [ClientInfo.namespace2.dbName],
      },
      Tasks: [
        {
          Name: TaskName,
          ForceCompareOnly: ForceCompare.bool,
          Task1: {
            IsRunTask: RunTask1.bool,
            Namespace: ClientInfo.namespace.dbName,
            FromDate: createStringDate(Start),
            ToDate: createStringDate(End),
            OrgUnit: Org,
          },
          Task2: {
            IsRunTask: RunTask2.bool,
            Namespace: ClientInfo.namespace2.dbName,
            FromDate: createStringDate(Start),
            ToDate: createStringDate(End),
            OrgUnit: Org,
          },
        },
      ],
    };

    // Below Code is to save INPUT FILE generated in Downloads, just for client side.

    const inputData = JSON.stringify(inputObject, null, 2);
    const blob = new Blob([inputData], { type: "text/plain" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.download = createFileType();
    link.href = url;
    link.click();
  };

  const createInputSCR = async () => {
    let namespace1Info = createNameSpace1Info();
    let namespace2Info = createNameSpace2Info();

    const inputObject = {
      ControlDBNamespace: {
        [namespace1Info]: [ClientInfo.namespace.dbName],
        [namespace2Info]: [ClientInfo.namespace2.dbName],
      },
      Tasks: [
        {
          Name: TaskName,
          ForceCompareOnly: ForceCompare.bool,
          Task1: {
            IsRunTask: RunTask1.bool,
            Namespace: ClientInfo.namespace.dbName,
            DateRelativeToToday: createStringDate(DateRelative),
            OrgUnit: Org,
          },
          Task2: {
            IsRunTask: RunTask2.bool,
            Namespace: ClientInfo.namespace2.dbName,
            DateRelativeToToday: createStringDate(DateRelative),
            OrgUnit: Org,
          },
        },
      ],
    };

    // Below Code is to save INPUT FILE generated in Downloads, just for client side.

    const inputData = JSON.stringify(inputObject, null, 2);
    const blob = new Blob([inputData], { type: "text/plain" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.download = createFileType();
    link.href = url;
    link.click();
  };

  const createInputAE = async () => {
    let namespace1Info = createNameSpace1Info();
    let namespace2Info = createNameSpace2Info();

    const inputObject = {
      ControlDBNamespace: {
        [namespace1Info]: [ClientInfo.namespace.dbName],
        [namespace2Info]: [ClientInfo.namespace2.dbName],
      },
      Tasks: [
        {
          Name: TaskName,
          ForceCompareOnly: ForceCompare.bool,
          Task1: {
            IsRunTask: RunTask1.bool,
            Namespace: ClientInfo.namespace.dbName,
            FromDate: createStringDate(Start),
            ToDate: createStringDate(End),
            OrgUnit: Org,
            Policy: PolicyName,
          },
          Task2: {
            IsRunTask: RunTask2.bool,
            Namespace: ClientInfo.namespace2.dbName,
            FromDate: createStringDate(Start),
            ToDate: createStringDate(End),
            OrgUnit: Org,
            Policy: PolicyName,
          },
        },
      ],
    };

    // Below Code is to save INPUT FILE generated in Downloads, just for client side.

    const inputData = JSON.stringify(inputObject, null, 2);
    const blob = new Blob([inputData], { type: "text/plain" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.download = createFileType();
    link.href = url;
    link.click();
  };

  const createInputEFile = async () => {
    let namespace1Info = createNameSpace1Info();
    let namespace2Info = createNameSpace2Info();

    const inputObject = {
      ControlDBNamespace: {
        [namespace1Info]: [ClientInfo.namespace.dbName],
        [namespace2Info]: [ClientInfo.namespace2.dbName],
      },
      Tasks: [
        {
          Name: TaskName,
          ForceCompareOnly: ForceCompare.bool,
          Task1: {
            IsRunTask: RunTask1.bool,
            Namespace: ClientInfo.namespace.dbName,
            PayGroupCalendarId: parseInt(PGCalendar),
            ExportMode: ModeExport,
            MockTransmit: TransmitMock.bool,
            ExportFileName: ExportFileName,
          },
          Task2: {
            IsRunTask: RunTask2.bool,
            Namespace: ClientInfo.namespace2.dbName,
            PayGroupCalendarId: parseInt(PGCalendar),
            ExportMode: ModeExport,
            MockTransmit: TransmitMock.bool,
            ExportFileName: ExportFileName,
          },
        },
      ],
    };

    // Below Code is to save INPUT FILE generated in Downloads, just for client side.

    const inputData = JSON.stringify(inputObject, null, 2);
    const blob = new Blob([inputData], { type: "text/plain" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.download = createFileType();
    link.href = url;
    link.click();
  };

  const createInputE = async () => {
    let namespace1Info = createNameSpace1Info();
    let namespace2Info = createNameSpace2Info();

    const inputObject = {
      ControlDBNamespace: {
        [namespace1Info]: [ClientInfo.namespace.dbName],
        [namespace2Info]: [ClientInfo.namespace2.dbName],
      },
      Tasks: [
        {
          Name: TaskName,
          ForceCompareOnly: ForceCompare.bool,
          Task1: {
            IsRunTask: RunTask1.bool,
            Namespace: ClientInfo.namespace.dbName,
            PayGroupCalendarId: parseInt(PGCalendar),
            ExportMode: ModeExport,
            MockTransmit: TransmitMock.bool,
          },
          Task2: {
            IsRunTask: RunTask2.bool,
            Namespace: ClientInfo.namespace2.dbName,
            PayGroupCalendarId: parseInt(PGCalendar),
            ExportMode: ModeExport,
            MockTransmit: TransmitMock.bool,
          },
        },
      ],
    };

    // Below Code is to save INPUT FILE generated in Downloads, just for client side.

    const inputData = JSON.stringify(inputObject, null, 2);
    const blob = new Blob([inputData], { type: "text/plain" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.download = createFileType();
    link.href = url;
    link.click();
  };

  const saveInput = async () => {
    let InputFile = createFileType();

    // POST Request to Send Input Parameter to Server Side and Database and then using these Input Parameters to run Job
    // and Generate Comparison Ouput and then save it to Output Table Client Specific.

    let url = inputAPI;
    let date = new Date().toLocaleString();

    // Convert Month to Type MM

    let month_temp = date.split("/")[0];
    let month;

    if (month_temp.length === 1) {
      month = "0" + month_temp;
    } else {
      month = month_temp;
    }

    // Convert Date to Type dd

    let d_temp = date.split("/")[1];
    let d;

    if (d_temp.length === 1) {
      d = "0" + d_temp;
    } else {
      d = d_temp;
    }

    let year_time = date.split("/")[2];

    // Make new date string in MM/dd/yyyy format with Time.

    let formatted_date = month + "/" + d + "/" + year_time;

    await axios({
      method: "POST",
      url: url,
      data: {
        User_ID: userInfo.samAccountName,
        User_Name: userInfo.displayName,
        User_Email: userInfo.userPrincipalName,
        Client: ClientInfo.clientName,
        DBName_1: ClientInfo.namespace.dbName,
        DBName_2: ClientInfo.namespace2.dbName,
        ControlDB_1: ClientInfo.namespace.controlDb,
        ControlDB_2: ClientInfo.namespace2.controlDb,
        ControlDBServer_1: ClientInfo.namespace.controlDbServer,
        ControlDBServer_2: ClientInfo.namespace2.controlDbServer,
        ControlDBServer_Server1: createNameSpace1Info(),
        ControlDBServer_Server2: createNameSpace2Info(),
        Task_Name: TaskName,
        ForceCompareOnly: ForceCompare.bool,
        RunTask_1: RunTask1.bool,
        RunTask_2: RunTask2.bool,
        Start_Time: Start === null ? "" : createStringDate(Start),
        End_Time: End === null ? "" : createStringDate(End),
        Date_Relative_To_Today:
          DateRelative === null ? "" : createStringDate(DateRelative),
        Org: Org,
        Policy: PolicyName === "" ? "" : PolicyName,
        Pay_Group_Calendar_Id: PGCalendar === "" ? 0 : parseInt(PGCalendar),
        Export_Mode: ModeExport === "" ? "" : ModeExport,
        Mock_Transmit: TransmitMock.bool === null ? false : TransmitMock.bool,
        Export_File_Name: ExportFileName,
        Job: Job.file,
        Date: formatted_date,
        LogId1: "",
        Status1: "",
        LogId2: "",
        Status2: "",
        Results: "Comparison Not Started",
        Compared: 0,
        Analyzed: 0,
        Version1_Path: "",
        Version2_Path: "",
        Analyze_Path: "",
      },
    })
      .then(() => {
        console.log(
          InputFile +
            "-Input POST Request is Succesfull and Input is added to Input Table"
        );
      })
      .catch((err) => {
        console.log("Error in " + InputFile + " POST Request", err);
      });
  };

  return (
    <>
      <FormControl className={classes.formControl}>
        <Box align="center" mt={1} mb={1}>
          <Grid container spacing={3}>
            <Grid item lg={12} md={12} xl={12} xs={12}>
              <FormLabel component="legend">
                Only Download Input JSON File?
              </FormLabel>
              <RadioGroup
                aria-label="download"
                name="controlled-radio-buttons-group"
                value={download}
                onChange={(e) => {
                  setdownload(e.target.value);

                  if (e.target.value === "true") {
                    setbuttonType("Download Input");
                  } else {
                    setbuttonType("START");
                  }

                  setSubOpen(false);
                }}
              >
                <FormControlLabel
                  value="true"
                  control={<Radio />}
                  label="Yes"
                />
                <FormControlLabel
                  value="false"
                  control={<Radio />}
                  label="No"
                />
              </RadioGroup>
            </Grid>

            <Grid item lg={12} md={12} xl={12} xs={12}>
              {Reset ? (
                <Button
                  id="resetButton"
                  variant="contained"
                  onClick={() => {
                    reset();
                    setOpen(false);
                    setSuccess(true);
                    setSubOpen(false);
                    setReset(false);
                    setdownload("false");
                  }}
                >
                  RESET
                </Button>
              ) : (
                <></>
              )}
            </Grid>

            <Grid item lg={12} md={12} xl={12} xs={12}>
              {!subOpen ? (
                <Button
                  disabled={
                    ClientInfo.namespace.dbName ===
                      ClientInfo.namespace2.dbName ||
                    Start > End ||
                    (!RunTask1.bool && download === "false") ||
                    (!RunTask2.bool && download === "false")
                  }
                  id="submitButton"
                  variant="contained"
                  color="primary"
                  onClick={() => {
                    if (download === "true") {
                      if (
                        Job.file === "PSR" ||
                        Job.file === "JSR" ||
                        Job.file === "JobStepRecalc"
                      ) {
                        createInputPSRJSRBRR(); // To save JSON file in Client Computer
                      } else if (Job.file === "SCR") {
                        createInputSCR(); // To save JSON file in Client Computer
                      } else if (Job.file === "AE_Sample") {
                        createInputAE(); // To save JSON file in Client Computer
                      } else if (Job.file === "Export" && ExportFileName !== "") {
                        createInputEFile(); // To save JSON file in Client Computer
                      }
                      else if (Job.file === "Export" && ExportFileName === "") {
                        createInputE(); // To save JSON file in Client Computer
                      }
                    } else {
                      saveInput(); // To save Input Parameter in Database
                    }

                    ComparisonRan();
                  }}
                >
                  {buttonType}
                </Button>
              ) : (
                <Button
                  disabled={true}
                  id="submitButton"
                  variant="contained"
                  color="primary"
                >
                  {buttonType}
                </Button>
              )}
            </Grid>

            <Grid item lg={12} md={12} xl={12} xs={12}>
              {subOpen ? (
                <p
                  id="submitMsg"
                  style={{
                    marginBottom: "0px",
                    marginTop: "5px",
                    fontSize: "14px",
                    textAlign: "center",
                    color: "#000000",
                  }}
                >
                  {msg}
                </p>
              ) : (
                <></>
              )}
            </Grid>
          </Grid>

          <Snackbar autoHideDuration={5000} open={open} onClose={handleClose}>
            <Alert
              id="submitResult"
              onClose={handleClose}
              severity={success ? "success" : "error"}
            >
              {success
                ? "Congratulations " +
                  userInfo.givenName +
                  "! Your request between Versions - " +
                  ClientInfo.namespace.dbName +
                  " and " +
                  ClientInfo.namespace2.dbName +
                  " of Client - " +
                  ClientInfo.clientName +
                  " targeting " +
                  Job.jobName +
                  " Job, has started successfully "
                : "Oops! We faced an issue processing your request. Sorry for the inconvenience"}
            </Alert>
          </Snackbar>
        </Box>
      </FormControl>
    </>
  );
};

export default ButtonCompare;
