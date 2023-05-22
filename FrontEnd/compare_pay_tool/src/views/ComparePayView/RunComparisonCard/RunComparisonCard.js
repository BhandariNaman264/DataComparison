import React from "react";
import {
  Box,
  Typography,
  Card,
  CardContent,
  Tooltip,
  Divider,
  makeStyles,
} from "@material-ui/core";
import "react-loader-spinner/dist/loader/css/react-spinner-loader.css";
import ButtonCompare from "./components/RunComparison";

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
    minHeight: 200,
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

const RunComparisonCard = ({
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
  return (
    <Card className={classes.card}>
      <Box mt={2} mb={2}>
        <Tooltip title="Click START to Run Comparison">
          <Typography variant="h5" align="center" color="primary">
            Run Comparison
          </Typography>
        </Tooltip>
      </Box>
      <Divider />
      <CardContent>
        <Box mt={2} mb={2}>
          {((Job.id === 1 || Job.id === 2 || Job.id === 4) &&
            ClientInfo.namespace &&
            ClientInfo.namespace2 &&
            ForceCompare.booleanValue !== "" &&
            RunTask1.booleanValue !== "" &&
            RunTask2.booleanValue !== "" &&
            Job.id !== 0 &&
            Start &&
            End &&
            TaskName !== "") ||
          (ClientInfo.id > 1 && ClientInfo.clientId > 0) ? (
            <ButtonCompare
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
          ) : (Job.id === 3 &&
              ClientInfo.namespace &&
              ClientInfo.namespace2 &&
              ForceCompare.booleanValue !== "" &&
              RunTask1.booleanValue !== "" &&
              RunTask2.booleanValue !== "" &&
              Job.id !== 0 &&
              DateRelative &&
              TaskName !== "") ||
            (ClientInfo.id > 1 && ClientInfo.clientId > 0) ? (
            <ButtonCompare
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
          ) : (Job.id === 6 &&
              ClientInfo.namespace &&
              ClientInfo.namespace2 &&
              ForceCompare.booleanValue !== "" &&
              RunTask1.booleanValue !== "" &&
              RunTask2.booleanValue !== "" &&
              Job.id !== 0 &&
              Start &&
              End &&
              TaskName !== "") ||
            (ClientInfo.id > 1 && ClientInfo.clientId > 0) ? (
            <ButtonCompare
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
          ) : (Job.id === 5 &&
              ClientInfo.namespace &&
              ClientInfo.namespace2 &&
              ForceCompare.booleanValue === "False" &&
              RunTask1.booleanValue !== "" &&
              RunTask2.booleanValue !== "" &&
              Job.id !== 0 &&
              ModeExport !== "" &&
              PGCalendar !== "" &&
              TransmitMock.booleanValue !== "" &&
              TaskName !== "") ||
            (ClientInfo.id > 1 && ClientInfo.clientId > 0) ? (
            <ButtonCompare
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
          ) : (
            <></>
          )}
        </Box>
      </CardContent>
    </Card>
  );
};

export default RunComparisonCard;
