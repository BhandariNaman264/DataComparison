import React from "react";
import {
  Box,
  Grid,
  Typography,
  Card,
  CardContent,
  Tooltip,
  Divider,
  makeStyles,
} from "@material-ui/core";
import "react-loader-spinner/dist/loader/css/react-spinner-loader.css";
import ForceCompareOnly from "./components/ForceCompareOnly";
import IsRunTask1 from "./components/IsRunTask1";
import IsRunTask2 from "./components/IsRunTask2";
import OrgUnit from "./components/OrgUnit";
import TimeStart from "./components/TimeStart";
import TimeEnd from "./components/TimeEnd";
import Name from "./components/Name";
import DateRelativeToToday from "./components/DateRelativeToToday";
import Policy from "./components/Policy";
import PGCalendarID from "./components/PGCalendarID";
import MockTransmit from "./components/MockTransmit";
import ExportFile from "./components/ExportFile";
import ExportMode from "./components/ExportMode";

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
    minHeight: 600,
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

const ConfriguationsCard = ({
  ForceCompare,
  setForceCompare,
  RunTask1,
  setRunTask1,
  RunTask2,
  setRunTask2,
  Org,
  TaskName,
  setTaskName,
  setOrg,
  Start,
  setStart,
  End,
  setEnd,
  setSubOpen,
  DateRelative,
  setDateRelative,
  Job,
  setJob,
  PolicyName,
  setPolicyName,
  PGCalendar,
  setPGCalendar,
  TransmitMock,
  setTransmitMock,
  ModeExport,
  setModeExport,
  ExportFileName,
  setExportFileName,
}) => {
  const classes = useStyles();

  return (
    <Card className={classes.card}>
      <Box mt={2} mb={2}>
        <Tooltip title="Select Confriguations for the Comparison">
          <Typography variant="h5" align="center" color="primary">
            Comparison Confriguations
          </Typography>
        </Tooltip>
      </Box>
      <Divider />
      <CardContent>
        <Box mt={2} mb={2}>
          <Grid container spacing={3}>
            <Grid item lg={12} md={12} xl={12} xs={12}>
              <Name
                TaskName={TaskName}
                setTaskName={setTaskName}
                setSubOpen={setSubOpen}
              />
            </Grid>

            <Grid item lg={12} md={12} xl={12} xs={12}>
              <ForceCompareOnly
                ForceCompare={ForceCompare}
                setForceCompare={setForceCompare}
                setSubOpen={setSubOpen}
              />
            </Grid>

            <Grid item lg={6} md={6} xl={6} xs={6}>
              <IsRunTask1
                RunTask1={RunTask1}
                setRunTask1={setRunTask1}
                setSubOpen={setSubOpen}
              />
            </Grid>

            <Grid item lg={6} md={6} xl={6} xs={6}>
              <IsRunTask2
                RunTask2={RunTask2}
                setRunTask2={setRunTask2}
                setSubOpen={setSubOpen}
              />
            </Grid>

            {Job.id === 1 || Job.id === 2 || Job.id === 4 || Job.id === 6 ? (
              <>
                <Grid item lg={6} md={6} xl={6} xs={6}>
                  <TimeStart
                    Start={Start}
                    setStart={setStart}
                    setSubOpen={setSubOpen}
                  />
                </Grid>

                <Grid item lg={6} md={6} xl={6} xs={6}>
                  <TimeEnd
                    End={End}
                    setEnd={setEnd}
                    setSubOpen={setSubOpen}
                    Start={Start}
                  />
                </Grid>
              </>
            ) : (
              <></>
            )}

            {Job.id === 3 ? (
              <>
                <Grid item lg={12} md={12} xl={12} xs={12}>
                  <DateRelativeToToday
                    DateRelative={DateRelative}
                    setDateRelative={setDateRelative}
                    setSubOpen={setSubOpen}
                  />
                </Grid>
              </>
            ) : (
              <></>
            )}

            {Job.id === 6 ? (
              <>
                <Grid item lg={12} md={12} xl={12} xs={12}>
                  <Policy
                    PolicyName={PolicyName}
                    setPolicyName={setPolicyName}
                    setSubOpen={setSubOpen}
                  />
                </Grid>
              </>
            ) : (
              <></>
            )}

            {Job.id === 5 ? (
              <>
                <Grid item lg={12} md={12} xl={12} xs={12}>
                  <PGCalendarID
                    PGCalendar={PGCalendar}
                    setPGCalendar={setPGCalendar}
                    setSubOpen={setSubOpen}
                  />
                </Grid>
                <Grid item lg={12} md={12} xl={12} xs={12}>
                  <MockTransmit
                    TransmitMock={TransmitMock}
                    setTransmitMock={setTransmitMock}
                    setSubOpen={setSubOpen}
                  />
                </Grid>
                <Grid item lg={6} md={6} xl={6} xs={12}>
                  <ExportFile
                    ExportFileName={ExportFileName}
                    setExportFileName={setExportFileName}
                    setSubOpen={setSubOpen}
                  />
                </Grid>
                <Grid item lg={6} md={6} xl={6} xs={12}>
                  <ExportMode
                    ModeExport={ModeExport}
                    setModeExport={setModeExport}
                    setSubOpen={setSubOpen}
                  />
                </Grid>
              </>
            ) : (
              <></>
            )}
            {Job.id !== 5 ? (
              <Grid item lg={12} md={12} xl={12} xs={12}>
                <OrgUnit Org={Org} setOrg={setOrg} setSubOpen={setSubOpen} />
              </Grid>
            ) : (
              <></>
            )}
          </Grid>
        </Box>
      </CardContent>
    </Card>
  );
};

export default ConfriguationsCard;
