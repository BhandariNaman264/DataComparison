import React, { useEffect, useState } from "react";
import {
  Box,
  FormControl,
  Snackbar,
  OutlinedInput,
  InputLabel,
  makeStyles,
  CircularProgress,
} from "@material-ui/core";
import { green } from "@material-ui/core/colors";
import axios from "axios";
import Loader from "react-loader-spinner";
import CheckCircleIcon from "@material-ui/icons/CheckCircle";
import ErrorIcon from "@material-ui/icons/Error";
import {
  fileCheckAPI,
  fileServerCheckAPI,
} from "src/components/APIBase/BaseURL";
import "react-loader-spinner/dist/loader/css/react-spinner-loader.css";

const useStyles = makeStyles((theme) => ({
  root: {
    backgroundColor: theme.palette.background.dark,
    minHeight: "100%",
    paddingBottom: theme.spacing(3),
    paddingTop: theme.spacing(3),
  },
  card: {
    height: "100%",
    color: theme.palette.text.secondary,
    minHeight: 688,
  },
  formControl: {
    margin: theme.spacing(1),
    minWidth: 450,
    maxWidth: 450,
  },
  sortFormControl: {
    maxWidth: 450,
    minWidth: 450,
  },
  sortGroup: {
    position: "absolute",
    right: 0,
    top: -40,
  },
  server: {
    wordWrap: "word-break",
    textAlign: "left",
    color: "rgba(0, 0, 0, 0.54)",
  },
  serverHead: {
    color: "rgba(0, 0, 0, 0.83)",
    textAlign: "left",
    marginTop: 8,
    marginBottom: 3,
  },
}));

const useStyleSpinner = makeStyles((theme) => ({
  root: {
    position: "relative",
  },
  bottom: {
    color: theme.palette.grey[theme.palette.type === "light" ? 200 : 700],
  },
  top: {
    color: "#3f51b5",
    animationDuration: "550ms",
    position: "absolute",
    left: 0,
  },
  circle: {
    strokeLinecap: "round",
  },
}));

function CircleProgress(props) {
  const classes = useStyleSpinner();

  return (
    <div className={classes.root}>
      <CircularProgress
        variant="determinate"
        className={classes.bottom}
        size={40}
        thickness={4}
        {...props}
        value={100}
      />
      <CircularProgress
        variant="indeterminate"
        disableShrink
        className={classes.top}
        classes={{ circle: classes.circle }}
        size={40}
        thickness={4}
        {...props}
      />
    </div>
  );
}

const ClientFilePath = ({
  ClientInfo,
  setClientInfo,
  valid,
  setValid,
  Alert,
  setSubOpen,
}) => {
  const classes = useStyles();
  const [checking, setChecking] = useState(false);
  const [open, setOpen] = useState(false);
  const [tmpFilePath, setTmpFilePath] = useState("");
  const [serverPath, setServerPath] = useState({
    server: "",
    path: "",
    size: "",
  });
  const [servLoad, setServLoad] = useState(false);

  const handleInput = (event) => {
    setValid(0);
    setSubOpen(false);
    setClientInfo({ ...ClientInfo, [event.target.name]: event.target.value });
  };

  const handleClose = (event, reason) => {
    if (reason === "clickaway") {
      return;
    }
    setOpen(false);
  };

  useEffect(() => {
    setTmpFilePath("");
  }, [ClientInfo.id]);

  useEffect(() => {
    if (valid === -1) {
      setServLoad(true);
      console.log("test");
      let url = fileServerCheckAPI;
      async function serverCheck() {
        await axios({
          method: "GET",
          url,
        })
          .then(function (response) {
            setServerPath(response.data);
            console.log("fileServerCheckAPI in FilePath Success");
          })
          .catch(function (error) {
            console.log(error);
            console.log("fileServerCheckAPI in File Path");
          })
          .finally(() => {
            setServLoad(false);
          });
      }
      serverCheck();
    }
  }, [valid]);

  const checkFilePath = () => {
    // only run check if the user input a value
    setOpen(false);
    if (ClientInfo.filePath) {
      setTmpFilePath(ClientInfo.filePath);
      // check if the user actually changed the input value
      if (ClientInfo.filePath !== tmpFilePath || valid === -4) {
        // check that the filepath leads to a .bak file
        if (
          !ClientInfo.filePath.substr(0, 2).includes("\\\\") ||
          ClientInfo.filePath
            .substr(2, ClientInfo.filePath.length - 1)
            .split("\\").length < 2
        ) {
          // check that the filepath starts with '\\' for server path UNC
          setValid(-3);
        } else {
          // check validity
          setValid(0);
          setChecking(true);
          let url = fileCheckAPI + ClientInfo.filePath;
          async function validFileCheck() {
            await axios({
              method: "GET",
              url,
              "Access-Control-Allow-Credentials": true,
            })
              .then(function (response) {
                console.log("fileCheckAPI in FilePath Success");
                // response will return two properties: file (string) and exist (boolean)
                if (!response.data.exist) {
                  setValid(-1);
                  setOpen(true);
                } else {
                  // check that the inputted file is a .bak file
                  setValid(
                    ClientInfo.filePath.substr(
                      ClientInfo.filePath.length - 4
                    ) !== ".bak"
                      ? -2
                      : 1
                  );
                  if (
                    ClientInfo.filePath.substr(ClientInfo.filePath.length - 4)
                  ) {
                    setOpen(true);
                  }
                }
              })
              .catch(function (error) {
                setOpen(true);
                setValid(-4);
                console.log(error);
                console.log("fileCheckAPI in FilePath");
              })
              .finally(() => {
                setChecking(false);
              });
          }
          validFileCheck();
        }
      }
    } else {
      setTmpFilePath("");
      setValid(0);
    }
  };

  return (
    <>
      <Box mb={2}>
        <FormControl className={classes.formControl} variant="outlined">
          <InputLabel required error={valid < 0} htmlFor="backupFilePath">
            Client File Path
          </InputLabel>
          <OutlinedInput
            id="clientBackupFilePath"
            name="filePath"
            placeholder="\\servername\filename.bak"
            value={ClientInfo.filePath}
            onChange={handleInput}
            onBlur={checkFilePath}
            error={valid < 0}
            endAdornment={
              checking ? (
                <CircleProgress />
              ) : valid === -2 ? (
                <ErrorIcon style={{ color: "#f44336" }} />
              ) : valid === 1 ? (
                <CheckCircleIcon style={{ color: green[500] }} />
              ) : (
                ""
              )
            }
            labelWidth={75}
          />
          <p
            id="filePathResult"
            style={{
              color:
                valid === 1
                  ? "rgb(76, 175, 80)"
                  : valid < 0
                  ? "#f44336"
                  : "rgba(0, 0, 0, 0.54)",
              marginBottom: "0px",
              marginTop: "5px",
              fontSize: "14px",
              textAlign: "left",
            }}
          >
            {checking
              ? "Checking file path for accessibility..."
              : valid === 1
              ? "File path is valid and accessible"
              : valid === -1
              ? "File cannot be accessed from our file servers"
              : valid === -2
              ? "File exists, but must be a .bak file"
              : valid === -3
              ? "File path does not follow server UNC"
              : valid === -4
              ? "Check failed, try again"
              : "Please provide a valid UNC backup file path to our File Servers"}
          </p>
          {valid === -1 || valid === -2 || valid === -4 ? (
            <Snackbar open={open} onClose={handleClose} autoHideDuration={6000}>
              <Alert
                id="filePathError"
                onClose={handleClose}
                severity={valid === -2 ? "warning" : "error"}
              >
                {valid === -2
                  ? "Error: Backup file must be a .bak file. Please rename the file to the appropriate file type."
                  : valid === -4
                  ? "There was an error in checking your file. Please try again."
                  : "Error: Backup file is inaccessible. Make sure that your file has been copied to our accessible file servers."}
              </Alert>
            </Snackbar>
          ) : (
            <></>
          )}
          {valid === -1 ? (
            servLoad ? (
              <Box mt={2}>
                <div style={{ justifyContent: "center", marginLeft: "1em" }}>
                  <Loader
                    type="ThreeDots"
                    color="#00BFFF"
                    height={50}
                    width={50}
                    timeout={20000}
                  />
                </div>
              </Box>
            ) : (
              <Box mt={1} mb={1}>
                <p className={classes.serverHead}>Recommended Server Path: </p>
                <p
                  id="serverPath"
                  className={classes.server}
                  style={{ wordWrap: "break-word" }}
                >
                  {serverPath.path}
                </p>
                <p className={classes.serverHead}>
                  Available Space:{" "}
                  <span className={classes.server} id="serverSpace">
                    {serverPath.size}
                  </span>
                </p>
              </Box>
            )
          ) : (
            <></>
          )}
        </FormControl>
      </Box>
    </>
  );
};

export default ClientFilePath;
