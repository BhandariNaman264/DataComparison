import React, { useState, useEffect } from "react";
import { Card, Typography, makeStyles } from "@material-ui/core";
import Paper from "@material-ui/core/Paper";
import "react-loader-spinner/dist/loader/css/react-spinner-loader.css";
import axios from "axios";
import { CPTtracerAPI, tracerAPI } from "src/components/APIBase/BaseURL";
import { useParams } from "react-router-dom";

const useStyles = makeStyles((theme) => ({
  root: {
    width: "100%",
  },
  paper: {
    width: "100%",
    marginBottom: theme.spacing(2),
  },
  table: {
    minWidth: 750,
  },
  visuallyHidden: {
    border: 0,
    clip: "rect(0 0 0 0)",
    height: 1,
    margin: -1,
    overflow: "hidden",
    padding: 0,
    position: "absolute",
    top: 20,
    width: 1,
  },
  formControl: {
    margin: 8,
    marginTop: 16,
    marginBottom: 0,
    minWidth: 350,
  },
}));

const Tracer = () => {
  const classes = useStyles();

  //   const [result, setresult] = useState([]);

  const { id } = useParams();

  const [analyze, setanalyze] = useState({
    id: null,
    comparison_ID: null,
    line: null,
    client: "",
    employee_ID: "",
    buisness_Date: "",
    employee_Pay_AdjustID: "",
    discrepancy: "",
  });

  useEffect(() => {
    let analyzeApi = async () => {
      let analyze_id = id;
      let url = CPTtracerAPI + analyze_id;
      await axios({
        method: "GET",
        url,
      })
        .then(function (response) {
          setanalyze(response.data);
          console.log(response.data);
        })
        .catch(function (error) {
          console.log(error);
        });
    };
    let tracerApi = async () => {
      let employee = parseInt(analyze.employee_ID, 10);
      let url = tracerAPI;
      await axios({
        method: "GET",
        url: url,
        data: {
          EmployeeId: employee,
          Date: analyze.buisness_Date,
          RunPeriod: "DAILY",
          ShowPolicyConfiguration: true,
          ShowOnlyReferencePolicyConfiguration: true,
          SegmentCapturePeriodType: "DAILY",
          KeepSegmentsCaptureRunPeriod: false,
          KeepRunsOutsideRunPeriod: false,
          DontAddSegmentsForRepeatRuns: true,
          DropEmptyRuns: false,
          UseClientRounding: true,
          CalculateMissingAmounts: true,
        },
      })
        .then(function (response) {
          //   setresult(response.data);
          console.log(response.data);
        })
        .catch(function (error) {
          console.log(error);
        });
    };
    analyzeApi();
    tracerApi();
  }, [id, analyze.employee_ID, analyze.buisness_Date]);

  return (
    <Card>
      <Typography
        variant="h2"
        align="center"
        color="primary"
        style={{ margin: "2rem" }}
      >
        Tracer
      </Typography>
      <Paper className={classes.paper}></Paper>
    </Card>
  );
};

export default Tracer;
