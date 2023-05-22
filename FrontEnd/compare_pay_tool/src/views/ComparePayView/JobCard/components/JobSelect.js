import React from "react";
import { Box, FormControl, TextField, makeStyles } from "@material-ui/core";
import Autocomplete from "@material-ui/lab/Autocomplete";
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

const JobSelect = ({ Job, setJob, setSubOpen, jobchangereset }) => {
  const classes = useStyles();
  //6 options for selecting the job
  const jobList = [
    { jobName: "Pay Summary Recalc", id: 1, file: "PSR" },
    { jobName: "Base Rate Recalc", id: 2, file: "BRR" },
    { jobName: "Schedule Cost Recalc", id: 3, file: "SCR" },
    { jobName: "Job Step Recalc", id: 4, file: "JobStepRecalc" },
    { jobName: "Pay Export", id: 5, file: "Export" },
    { jobName: "Award Entitlement", id: 6, file: "AE_Sample" },
  ];
  return (
    <Box mb={2}>
      <FormControl required className={classes.formControl}>
        <Autocomplete
          id="job"
          options={jobList}
          value={Job}
          required
          getOptionSelected={(option, value) => option.id === value.id}
          getOptionLabel={(option) =>
            option.id === 1
              ? "Pay Summary Recalc"
              : option.id === 2
              ? "Base Rate Recalc"
              : option.id === 3
              ? "Schedule Cost Recalc"
              : option.id === 4
              ? "Job Step Recalc"
              : option.id === 5
              ? "Pay Export"
              : option.id === 6
              ? "Award Entitlement"
              : ""
          }
          onChange={(event, newValue) => {
            setJob((prev) => {
              let update = { ...prev };
              update.jobName = newValue ? newValue.jobName : "";
              update.id = newValue ? newValue.id : 0;
              update.file = newValue ? newValue.file : "";
              return update;
            });
            jobchangereset();
            setSubOpen(false);
          }}
          renderInput={(params) => (
            <TextField {...params} label="Job" required variant="outlined" />
          )}
        />
      </FormControl>
    </Box>
  );
};

export default JobSelect;
