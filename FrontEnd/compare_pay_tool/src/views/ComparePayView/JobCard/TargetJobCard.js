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
import JobSelect from "./components/JobSelect";

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

const TargetJobCard = ({ Job, setJob, setSubOpen, jobchangereset }) => {
  const classes = useStyles();
  return (
    <Card className={classes.card}>
      <Box mt={2} mb={2}>
        <Tooltip title="Select Target Job">
          <Typography variant="h5" align="center" color="primary">
            Target Job
          </Typography>
        </Tooltip>
      </Box>
      <Divider />
      <CardContent>
        <Box mt={2} mb={2}>
          <JobSelect
            Job={Job}
            setJob={setJob}
            setSubOpen={setSubOpen}
            jobchangereset={jobchangereset}
          />
        </Box>
      </CardContent>
    </Card>
  );
};

export default TargetJobCard;
