import React, { useState, useEffect } from "react";
import PropTypes from "prop-types";
import {
  Card,
  CardContent,
  Container,
  Grid,
  Chip,
  TextField,
  FormControl,
  Divider,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  Typography,
  TableSortLabel,
  Button,
  makeStyles,
} from "@material-ui/core";
import TableContainer from "@material-ui/core/TableContainer";
import TablePagination from "@material-ui/core/TablePagination";
import { dates } from "src/util/DateTime/dates";
import Paper from "@material-ui/core/Paper";
import Loader from "react-loader-spinner";
import "react-loader-spinner/dist/loader/css/react-spinner-loader.css";
import axios from "axios";
import {
  analyzeAPI,
  inputAPI,
  analyzeBRRAPI,
  analyzeJSRAPI,
  analyzeSCRAPI,
  analyzeAEAPI,
  analyzeEAPI,
} from "src/components/APIBase/BaseURL";
import { Link } from "react-router-dom";
import DateFnsUtils from "@date-io/date-fns";
import {
  MuiPickersUtilsProvider,
  KeyboardDatePicker,
} from "@material-ui/pickers";
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

function descendingComparator(a, b, orderBy) {
  var compA =
    typeof a[orderBy] === "string"
      ? a[orderBy].toLowerCase().trim()
      : a[orderBy];
  var compB =
    typeof b[orderBy] === "string"
      ? b[orderBy].toLowerCase().trim()
      : b[orderBy];
  if (compB < compA) {
    return -1;
  }
  if (compB > compA) {
    return 1;
  }
  return 0;
}

function getComparator(order, orderBy) {
  return order === "desc"
    ? (a, b) => descendingComparator(a, b, orderBy)
    : (a, b) => -descendingComparator(a, b, orderBy);
}

function stableSort(array, comparator) {
  const stabilizedThis = array.map((el, index) => [el, index]);
  stabilizedThis.sort((a, b) => {
    const order = comparator(a[0], b[0]);
    if (order !== 0) return order;
    return a[1] - b[1];
  });
  return stabilizedThis.map((el) => el[0]);
}

const headCellsPSR_SCR = [
  { id: "line", numeric: false, label: "Line" },
  { id: "employee_id", numeric: false, label: "Employee ID" },
  { id: "date", numeric: false, label: "Date" },
  {
    id: "employee_adjustid",
    numeric: false,
    label: "Version 1 Employee Pay AdjustID - Version 2 Employee Pay AdjustID",
  },
  { id: "result", numeric: false, label: "Result" },
  { id: "tracer", numeric: false, label: "Tracer" },
];

function EnhancedTableHeadFilePSRSCR(props) {
  const { classes, order, orderBy, onRequestSort } = props;
  const createSortHandler = (property) => (event) => {
    onRequestSort(event, property);
  };

  return (
    <TableHead>
      <TableRow>
        {headCellsPSR_SCR.map((headCell) => (
          <TableCell
            key={headCell.id}
            style={{ fontWeight: 600, color: "#000000" }}
            align={headCell.numeric ? "right" : "left"}
            sortDirection={orderBy === headCell.id ? order : false}
          >
            <TableSortLabel
              active={orderBy === headCell.id}
              direction={orderBy === headCell.id ? order : "asc"}
              onClick={createSortHandler(headCell.id)}
            >
              {headCell.label}
              {orderBy === headCell.id ? (
                <span className={classes.visuallyHidden}>
                  {order === "desc" ? "sorted descending" : "sorted ascending"}
                </span>
              ) : null}
            </TableSortLabel>
          </TableCell>
        ))}
      </TableRow>
    </TableHead>
  );
}

EnhancedTableHeadFilePSRSCR.propTypes = {
  classes: PropTypes.object.isRequired,
  order: PropTypes.oneOf(["asc", "desc"]).isRequired,
  orderBy: PropTypes.string.isRequired,
};

const headCells2 = [
  { id: "version1_path", numeric: false, label: "Version 1 File" },
  { id: "version2_path", numeric: false, label: "Version 2 File" },
  { id: "analyze_path", numeric: false, label: "Analyze File" },
];

function EnhancedTableHeadFile(props) {
  const { classes, order, orderBy, onRequestSort } = props;
  const createSortHandler = (property) => (event) => {
    onRequestSort(event, property);
  };

  return (
    <TableHead>
      <TableRow>
        {headCells2.map((headCell) => (
          <TableCell
            key={headCell.id}
            style={{ fontWeight: 600, color: "#000000" }}
            align={headCell.numeric ? "right" : "center"}
            sortDirection={orderBy === headCell.id ? order : false}
          >
            <TableSortLabel
              active={orderBy === headCell.id}
              direction={orderBy === headCell.id ? order : "asc"}
              onClick={createSortHandler(headCell.id)}
            >
              {headCell.label}
              {orderBy === headCell.id ? (
                <span className={classes.visuallyHidden}>
                  {order === "desc" ? "sorted descending" : "sorted ascending"}
                </span>
              ) : null}
            </TableSortLabel>
          </TableCell>
        ))}
      </TableRow>
    </TableHead>
  );
}

EnhancedTableHeadFile.propTypes = {
  classes: PropTypes.object.isRequired,
  order: PropTypes.oneOf(["asc", "desc"]).isRequired,
  orderBy: PropTypes.string.isRequired,
};

const headCellsBRR = [
  { id: "line", numeric: false, label: "Line" },
  { id: "employeeid", numeric: false, label: "Employee ID" },
  { id: "employmentstatusid", numeric: false, label: "Employee Status ID" },
  {
    id: "start",
    numeric: false,
    label: "Effective Start",
  },
  { id: "end", numeric: false, label: "Effective End" },
  { id: "table", numeric: false, label: "Table" },
  { id: "discrepancy", numeric: false, label: "Discrepancy" },
];

function EnhancedTableHeadBRR(props) {
  const { classes, order, orderBy, onRequestSort } = props;
  const createSortHandler = (property) => (event) => {
    onRequestSort(event, property);
  };

  return (
    <TableHead>
      <TableRow>
        {headCellsBRR.map((headCell) => (
          <TableCell
            key={headCell.id}
            style={{ fontWeight: 600, color: "#000000" }}
            align={headCell.numeric ? "right" : "left"}
            sortDirection={orderBy === headCell.id ? order : false}
          >
            <TableSortLabel
              active={orderBy === headCell.id}
              direction={orderBy === headCell.id ? order : "asc"}
              onClick={createSortHandler(headCell.id)}
            >
              {headCell.label}
              {orderBy === headCell.id ? (
                <span className={classes.visuallyHidden}>
                  {order === "desc" ? "sorted descending" : "sorted ascending"}
                </span>
              ) : null}
            </TableSortLabel>
          </TableCell>
        ))}
      </TableRow>
    </TableHead>
  );
}

EnhancedTableHeadBRR.propTypes = {
  classes: PropTypes.object.isRequired,
  order: PropTypes.oneOf(["asc", "desc"]).isRequired,
  orderBy: PropTypes.string.isRequired,
};

const headCellsJSR = [
  { id: "line", numeric: false, label: "Line" },
  { id: "employeeid", numeric: false, label: "Employee ID" },
  {
    id: "employeeworkassignmentid",
    numeric: false,
    label: "Employee Work Assignment ID",
  },
  { id: "table", numeric: false, label: "Table" },
  { id: "discrepancy", numeric: false, label: "Discrepancy" },
];

function EnhancedTableHeadJSR(props) {
  const { classes, order, orderBy, onRequestSort } = props;
  const createSortHandler = (property) => (event) => {
    onRequestSort(event, property);
  };

  return (
    <TableHead>
      <TableRow>
        {headCellsJSR.map((headCell) => (
          <TableCell
            key={headCell.id}
            style={{ fontWeight: 600, color: "#000000" }}
            align={headCell.numeric ? "right" : "left"}
            sortDirection={orderBy === headCell.id ? order : false}
          >
            <TableSortLabel
              active={orderBy === headCell.id}
              direction={orderBy === headCell.id ? order : "asc"}
              onClick={createSortHandler(headCell.id)}
            >
              {headCell.label}
              {orderBy === headCell.id ? (
                <span className={classes.visuallyHidden}>
                  {order === "desc" ? "sorted descending" : "sorted ascending"}
                </span>
              ) : null}
            </TableSortLabel>
          </TableCell>
        ))}
      </TableRow>
    </TableHead>
  );
}

EnhancedTableHeadJSR.propTypes = {
  classes: PropTypes.object.isRequired,
  order: PropTypes.oneOf(["asc", "desc"]).isRequired,
  orderBy: PropTypes.string.isRequired,
};

const headCellsAE = [
  { id: "line", numeric: false, label: "Line" },
  { id: "employeeid", numeric: false, label: "Employee ID" },
  { id: "table", numeric: false, label: "Table" },
  { id: "discrepancy", numeric: false, label: "Discrepancy" },
];

function EnhancedTableHeadAE(props) {
  const { classes, order, orderBy, onRequestSort } = props;
  const createSortHandler = (property) => (event) => {
    onRequestSort(event, property);
  };

  return (
    <TableHead>
      <TableRow>
        {headCellsAE.map((headCell) => (
          <TableCell
            key={headCell.id}
            style={{ fontWeight: 600, color: "#000000" }}
            align={headCell.numeric ? "right" : "left"}
            sortDirection={orderBy === headCell.id ? order : false}
          >
            <TableSortLabel
              active={orderBy === headCell.id}
              direction={orderBy === headCell.id ? order : "asc"}
              onClick={createSortHandler(headCell.id)}
            >
              {headCell.label}
              {orderBy === headCell.id ? (
                <span className={classes.visuallyHidden}>
                  {order === "desc" ? "sorted descending" : "sorted ascending"}
                </span>
              ) : null}
            </TableSortLabel>
          </TableCell>
        ))}
      </TableRow>
    </TableHead>
  );
}

EnhancedTableHeadAE.propTypes = {
  classes: PropTypes.object.isRequired,
  order: PropTypes.oneOf(["asc", "desc"]).isRequired,
  orderBy: PropTypes.string.isRequired,
};

const headCellsE = [
  { id: "line", numeric: false, label: "Line" },
  { id: "discrepancy", numeric: false, label: "Discrepancy" },
];

function EnhancedTableHeadE(props) {
  const { classes, order, orderBy, onRequestSort } = props;
  const createSortHandler = (property) => (event) => {
    onRequestSort(event, property);
  };

  return (
    <TableHead>
      <TableRow>
        {headCellsE.map((headCell) => (
          <TableCell
            key={headCell.id}
            style={{ fontWeight: 600, color: "#000000" }}
            align={headCell.numeric ? "right" : "left"}
            sortDirection={orderBy === headCell.id ? order : false}
          >
            <TableSortLabel
              active={orderBy === headCell.id}
              direction={orderBy === headCell.id ? order : "asc"}
              onClick={createSortHandler(headCell.id)}
            >
              {headCell.label}
              {orderBy === headCell.id ? (
                <span className={classes.visuallyHidden}>
                  {order === "desc" ? "sorted descending" : "sorted ascending"}
                </span>
              ) : null}
            </TableSortLabel>
          </TableCell>
        ))}
      </TableRow>
    </TableHead>
  );
}

EnhancedTableHeadE.propTypes = {
  classes: PropTypes.object.isRequired,
  order: PropTypes.oneOf(["asc", "desc"]).isRequired,
  orderBy: PropTypes.string.isRequired,
};

const AnalyzeTable = () => {
  const classes = useStyles();

  const [order, setOrder] = useState("asc");
  const [orderBy, setOrderBy] = useState("analyzeRequestBy");

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);

  const [loading, setLoading] = useState(true);

  const [result, setresult] = useState([]);
  const [list, setlist] = useState([]);

  const [input, setinput] = useState({
    id: null,
    user_ID: "",
    user_Name: "",
    user_Email: "",
    client: "",
    dbName_1: "",
    dbName_2: "",
    controlDB_1: "",
    controlDB_2: "",
    controlDBServer_1: "",
    controlDBServer_2: "",
    controlDBServer_Server1: "",
    controlDBServer_Server2: "",
    forceCompareOnly: null,
    runTask_1: null,
    runTask_2: null,
    start_Time: "",
    end_Time: "",
    date_Relative_To_Today: "",
    org: "",
    policy: "",
    pay_Group_Calendar_Id: 0,
    export_Mode: "",
    mock_Transmit: null,
    export_File_Name: "",
    job: "",
    date: "",
    logId1: "",
    status1: "",
    logId2: "",
    status2: "",
    results: "",
    compared: 2,
    analyzed: -1,
    version1_Path: "",
    version2_Path: "",
    analyze_Path: "",
  });

  const { id } = useParams();

  const day = new Date();
  const [selectedDate, setSelectedDate] = useState(day);

  function searchForPSRSCR(toSearch) {
    var results = [];

    toSearch = trimString(toSearch.toLowerCase());

    for (var i = 0; i < result.length; i++) {
      if (
        result[i].employee_ID?.toLowerCase().includes(toSearch) ||
        result[i].buisness_Date?.toLowerCase().includes(toSearch) ||
        result[i].discrepancy?.toLowerCase().includes(toSearch) ||
        result[i].employee_Pay_AdjustID?.toLowerCase().includes(toSearch)
      ) {
        results.push(result[i]);
      }
    }
    return results;
  }

  function searchForBRR(toSearch) {
    var results = [];

    toSearch = trimString(toSearch.toLowerCase());

    for (var i = 0; i < result.length; i++) {
      if (
        result[i].employeeId?.toLowerCase().includes(toSearch) ||
        result[i].employmentStatusId?.toLowerCase().includes(toSearch) ||
        result[i].table?.toLowerCase().includes(toSearch)
      ) {
        results.push(result[i]);
      }
    }
    return results;
  }

  function searchForJSR(toSearch) {
    var results = [];

    toSearch = trimString(toSearch.toLowerCase());

    for (var i = 0; i < result.length; i++) {
      if (
        result[i].employeeId?.toLowerCase().includes(toSearch) ||
        result[i].employeeWorkAssignmentId?.toLowerCase().includes(toSearch) ||
        result[i].table?.toLowerCase().includes(toSearch)
      ) {
        results.push(result[i]);
      }
    }
    return results;
  }

  function searchForAE(toSearch) {
    var results = [];

    toSearch = trimString(toSearch.toLowerCase());

    for (var i = 0; i < result.length; i++) {
      if (
        result[i].employeeId?.toLowerCase().includes(toSearch) ||
        result[i].table?.toLowerCase().includes(toSearch)
      ) {
        results.push(result[i]);
      }
    }
    return results;
  }

  function searchForE(toSearch) {
    var results = [];

    toSearch = trimString(toSearch.toLowerCase());

    for (var i = 0; i < result.length; i++) {
      if (
        result[i].employeeId?.toLowerCase().includes(toSearch) ||
        result[i].table?.toLowerCase().includes(toSearch)
      ) {
        results.push(result[i]);
      }
    }
    return results;
  }

  function trimString(s) {
    var l = 0,
      r = s.length - 1;
    while (l < s.length && s[l] === " ") l++;
    while (r > l && s[r] === " ") r -= 1;
    return s.substring(l, r + 1);
  }

  const handleSearchChangePSRSCR = (event) => {
    if (event.target.value === "") {
      setlist(result);
    } else {
      setlist(searchForPSRSCR(event.target.value));
    }
    setPage(0);
  };

  const handleSearchChangeBRR = (event) => {
    if (event.target.value === "") {
      setlist(result);
    } else {
      setlist(searchForBRR(event.target.value));
    }
    setPage(0);
  };

  const handleSearchChangeJSR = (event) => {
    if (event.target.value === "") {
      setlist(result);
    } else {
      setlist(searchForJSR(event.target.value));
    }
    setPage(0);
  };

  const handleSearchChangeAE = (event) => {
    if (event.target.value === "") {
      setlist(result);
    } else {
      setlist(searchForAE(event.target.value));
    }
    setPage(0);
  };

  const handleSearchChangeE = (event) => {
    if (event.target.value === "") {
      setlist(result);
    } else {
      setlist(searchForE(event.target.value));
    }
    setPage(0);
  };

  const handleDateChangePSRSCR = (date) => {
    setLoading(true);
    setSelectedDate(date);

    var d = new Date(date);

    // document.getElementById("requestSearch").value = "";

    var date_filter = `${d.getFullYear()}-${dates.pad(
      d.getMonth() + 1,
      2
    )}-${dates.pad(d.getDate(), 2)}`;

    setlist([]);

    for (let i = 0; i < result.length; i++) {
      setlist(
        result.filter(
          (item) => item.buisness_Date.substring(0, 10) === date_filter
        )
      );
    }

    setLoading(false);
  };

  const handleRequestSort = (event, property) => {
    const isAsc = orderBy === property && order === "asc";
    setOrder(isAsc ? "desc" : "asc");
    setOrderBy(property);
    setPage(0);
  };

  const handleChangePage = (event, newPage) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event) => {
    setRowsPerPage(parseInt(event.target.value, 10));
    setPage(0);
  };

  useEffect(() => {
    let analyzeApi = async () => {
      let comparison_id = id;
      let url = analyzeAPI + comparison_id;
      await axios({
        method: "GET",
        url,
      })
        .then(function (response) {
          setresult(response.data);
          setlist(response.data);
        })
        .catch(function (error) {
          console.log(error);
        });
    };
    let analyzeBRRApi = async () => {
      let comparison_id = id;
      let url = analyzeBRRAPI + comparison_id;
      await axios({
        method: "GET",
        url,
      })
        .then(function (response) {
          setresult(response.data);
          setlist(response.data);
        })
        .catch(function (error) {
          console.log(error);
        });
    };
    let analyzeJSRApi = async () => {
      let comparison_id = id;
      let url = analyzeJSRAPI + comparison_id;
      await axios({
        method: "GET",
        url,
      })
        .then(function (response) {
          setresult(response.data);
          setlist(response.data);
        })
        .catch(function (error) {
          console.log(error);
        });
    };
    let analyzeSCRApi = async () => {
      let comparison_id = id;
      let url = analyzeSCRAPI + comparison_id;
      await axios({
        method: "GET",
        url,
      })
        .then(function (response) {
          setresult(response.data);
          setlist(response.data);
        })
        .catch(function (error) {
          console.log(error);
        });
    };
    let analyzeAEApi = async () => {
      let comparison_id = id;
      let url = analyzeAEAPI + comparison_id;
      await axios({
        method: "GET",
        url,
      })
        .then(function (response) {
          setresult(response.data);
          setlist(response.data);
        })
        .catch(function (error) {
          console.log(error);
        });
    };
    let analyzeEApi = async () => {
      let comparison_id = id;
      let url = analyzeEAPI + comparison_id;
      await axios({
        method: "GET",
        url,
      })
        .then(function (response) {
          setresult(response.data);
          setlist(response.data);
        })
        .catch(function (error) {
          console.log(error);
        });
    };
    let inputApi = async () => {
      let comparison_id = id;
      let url = inputAPI + comparison_id;
      await axios({
        method: "GET",
        url,
      })
        .then(function (response) {
          setinput(response.data);
          setLoading(false);
        })
        .catch(function (error) {
          console.log(error);
        });
    };
    inputApi();
    if (input.job === "PSR") {
      analyzeApi();
    } else if (input.job === "BRR") {
      analyzeBRRApi();
    } else if (input.job === "JobStepRecalc") {
      analyzeJSRApi();
    } else if (input.job === "SCR") {
      analyzeSCRApi();
    } else if (input.job === "AE_Sample") {
      analyzeAEApi();
    } else if (input.job === "Export") {
      analyzeEApi();
    }
  }, [id, input.job]);

  return (
    <>
      <Card>
        <Typography
          variant="h2"
          align="center"
          color="primary"
          style={{ margin: "2rem" }}
        >
          Records and Analyze File
        </Typography>
        <Paper className={classes.paper}>
          <CardContent style={{ paddingBottom: 16 }}>
            <Divider style={{ marginTop: 16, marginBottom: 16 }} />
            <TableContainer>
              <Table
                className={classes.table}
                aria-labelledby="tableTitle"
                aria-label="enhanced table"
              >
                <EnhancedTableHeadFile
                  classes={classes}
                  order={order}
                  orderBy={orderBy}
                  onRequestSort={handleRequestSort}
                />
                <TableBody>
                  <TableCell style={{ width: "23%" }}>
                    {input.version1_Path === "" ? (
                      <></>
                    ) : (
                      <a
                        style={{ marginLeft: 260 }}
                        href={input.version1_Path}
                        target="_blank"
                        rel="noreferrer"
                        className={classes.title}
                      >
                        Version 1 Records
                      </a>
                    )}
                  </TableCell>
                  <TableCell style={{ width: "23%" }}>
                    {input.version2_Path === "" ? (
                      <></>
                    ) : (
                      <a
                        style={{ marginLeft: 260 }}
                        href={input.version2_Path}
                        target="_blank"
                        rel="noreferrer"
                        className={classes.title}
                      >
                        Version 2 Records
                      </a>
                    )}
                  </TableCell>
                  <TableCell style={{ width: "23%" }}>
                    {input.analyze_Path === "" ? (
                      <></>
                    ) : (
                      <a
                        style={{ marginLeft: 260 }}
                        href={input.analyze_Path}
                        target="_blank"
                        rel="noreferrer"
                        className={classes.title}
                      >
                        Analyze Records
                      </a>
                    )}
                  </TableCell>
                </TableBody>
              </Table>
            </TableContainer>
          </CardContent>
        </Paper>
      </Card>
      {input.job === "PSR" || input.job === "SCR" ? (
        <>
          {" "}
          <Divider />
          <Divider />
          <Divider style={{ marginTop: 16, marginBottom: 16 }} />
          <Divider />
          <Divider />
          <Card>
            <Divider style={{ marginTop: 16, marginBottom: 16 }} />
            <Typography
              variant="h2"
              align="center"
              color="primary"
              style={{ margin: "2rem" }}
            >
              Analyze
            </Typography>
            <Typography
              variant="h2"
              align="center"
              color="primary"
              style={{ margin: "2rem" }}
            >
              Comparison ID-{id}
            </Typography>
            <Chip
              style={{
                width: "25% ",
                backgroundColor:
                  input.analyzed === 0
                    ? "#90ee90"
                    : input.analyzed === 1
                    ? "#eec300"
                    : input.analyzed === 2
                    ? "#00FF00"
                    : "#FFFFFF",
              }}
              color="black"
              label={
                input.analyzed === 0
                  ? "Status: Analyze has started successfully for this Comparison"
                  : input.analyzed === 1
                  ? "Status: Analyzing.... Discrepancies Detected will continue to be added"
                  : input.analyzed === 2
                  ? "Status: Completed Analyzing for this Comparison"
                  : ""
              }
              size="large"
            />
            <Divider style={{ marginTop: 16, marginBottom: 16 }} />
            <Paper className={classes.paper}>
              <CardContent style={{ paddingBottom: 16 }}>
                <Container maxWidth={false}>
                  <Grid container spacing={3}>
                    <Grid item lg={6} md={6} xl={6} xs={12}>
                      <FormControl style={{ float: "left" }}>
                        <MuiPickersUtilsProvider utils={DateFnsUtils}>
                          <Grid container justify="space-around">
                            <KeyboardDatePicker
                              disableToolbar
                              variant="inline"
                              format="yyyy-MM-dd"
                              margin="normal"
                              id="comparisonDateFilter"
                              disabled={result.length === 0}
                              disableFuture={true}
                              label="Filter by Discrepancy Date"
                              value={selectedDate}
                              onChange={handleDateChangePSRSCR}
                              KeyboardButtonProps={{
                                "aria-label": "change date",
                              }}
                              style={{ minWidth: 350 }}
                            />
                          </Grid>
                        </MuiPickersUtilsProvider>
                      </FormControl>
                    </Grid>
                    <Grid item lg={6} md={6} xl={6} xs={12}>
                      <FormControl
                        style={{ float: "right" }}
                        className={classes.formControl}
                      >
                        <TextField
                          size="small"
                          onChange={handleSearchChangePSRSCR}
                          disabled={result.length === 0}
                          id="comparisonSearch"
                          label="Search"
                          variant="outlined"
                          // disabled={true}
                        />
                      </FormControl>
                    </Grid>
                  </Grid>
                </Container>
                <Divider style={{ marginTop: 16, marginBottom: 16 }} />
                <TableContainer>
                  <Table
                    className={classes.table}
                    aria-labelledby="tableTitle"
                    aria-label="enhanced table"
                  >
                    <EnhancedTableHeadFilePSRSCR
                      classes={classes}
                      order={order}
                      orderBy={orderBy}
                      onRequestSort={handleRequestSort}
                    />
                    {loading ? (
                      <TableBody>
                        <TableRow align="right">
                          <TableCell colSpan={16}>
                            <Loader
                              type="ThreeDots"
                              color="#000000"
                              height={50}
                              width={50}
                              //timeout={10000}
                              style={{ marginLeft: "50%" }}
                            />
                          </TableCell>
                        </TableRow>
                      </TableBody>
                    ) : (
                      <TableBody>
                        {list.length === 0 ? (
                          <TableRow>
                            <TableCell colSpan={16}>
                              No Discrepancy or Issue Found
                            </TableCell>
                          </TableRow>
                        ) : (
                          stableSort(list, getComparator(order, orderBy))
                            .slice(
                              page * rowsPerPage,
                              page * rowsPerPage + rowsPerPage
                            )
                            .map((request) => {
                              let rule = "Find Rule";
                              if (
                                request.discrepancy ===
                                  "Cant' Analyze - Difference Reason: No Record for Version 2" ||
                                request.discrepancy ===
                                  "Cant' Analyze - Difference Reason: No Record for Version 1" ||
                                request.discrepancy ===
                                  "No Difference Reason: No Record exist for both Versions" ||
                                request.discrepancy ===
                                  "Difference Reason: Can't Find Record for both Versions" ||
                                request.discrepancy === "No Analyze Needed" ||
                                request.discrepancy === "Duplicate Tardy" ||
                                request.discrepancy ===
                                  "Version 1 and 2 has no corresponding aligned record, but these records are similar and exist as Duplicate Tardy" ||
                                request.discrepancy ===
                                  "Only EmployeePayAdjustId Mismatch" ||
                                request.discrepancy ===
                                  "Only EmployeePayAdjustId and PayAdjCodeId Mismatch" ||
                                request.discrepancy === "Overlapping Tardy" ||
                                request.discrepancy ===
                                  "Duplicate Work Assignment with Different Rates" ||
                                request.discrepancy.includes(
                                  "Version 1 has no corresponding record"
                                ) ||
                                request.discrepancy.includes(
                                  "Version 2 has no corresponding record"
                                ) ||
                                request.employee_ID.includes(" - ") ||
                                request.buisness_Date.includes(" - ") ||
                                request.discrepancy ===
                                  "Can't Analyze Reason: Large Record File"
                              ) {
                                rule = "Not Applicable";
                              }

                              const analyzePSRSCRRow = (
                                <>
                                  <TableCell style={{ width: "5%" }}>
                                    {request.line}
                                  </TableCell>
                                  <TableCell style={{ width: "3%" }}>
                                    {request.employee_ID}
                                  </TableCell>
                                  <TableCell style={{ width: "10%" }}>
                                    {request.buisness_Date}
                                  </TableCell>
                                  <TableCell style={{ width: "17%" }}>
                                    {request.employee_Pay_AdjustID}
                                  </TableCell>
                                  <TableCell style={{ width: "15%" }}>
                                    {request.discrepancy}
                                  </TableCell>
                                  {rule === "Not Applicable" ? (
                                    <TableCell style={{ width: "5%" }}>
                                      <Button
                                        disabled={rule === "Not Applicable"}
                                        id="tracer"
                                        variant="contained"
                                        color="primary"
                                        // onClick={() => {
                                        // }}
                                      >
                                        {rule}
                                      </Button>
                                    </TableCell>
                                  ) : (
                                    <TableCell style={{ width: "10%" }}>
                                      <Link to={`/rule/${request.id}`}>
                                        <Button
                                          disabled={rule === "Not Applicable"}
                                          id="tracer"
                                          variant="contained"
                                          color="primary"
                                          // onClick={() => {
                                          // }}
                                        >
                                          {rule}
                                        </Button>
                                      </Link>
                                    </TableCell>
                                  )}
                                </>
                              );

                              return (
                                <TableRow
                                  hover
                                  tabIndex={-1}
                                  key={request.index}
                                >
                                  {analyzePSRSCRRow}
                                </TableRow>
                              );
                            })
                        )}
                      </TableBody>
                    )}
                  </Table>
                </TableContainer>
                <TablePagination
                  rowsPerPageOptions={[10, 15, 25]}
                  component="div"
                  count={list.length}
                  rowsPerPage={rowsPerPage}
                  page={page}
                  onChangePage={handleChangePage}
                  onChangeRowsPerPage={handleChangeRowsPerPage}
                />
              </CardContent>
            </Paper>
          </Card>
        </>
      ) : input.job === "BRR" ? (
        <>
          {" "}
          <Divider />
          <Divider />
          <Divider style={{ marginTop: 16, marginBottom: 16 }} />
          <Divider />
          <Divider />
          <Card>
            <Divider style={{ marginTop: 16, marginBottom: 16 }} />
            <Typography
              variant="h2"
              align="center"
              color="primary"
              style={{ margin: "2rem" }}
            >
              Analyze
            </Typography>
            <Typography
              variant="h2"
              align="center"
              color="primary"
              style={{ margin: "2rem" }}
            >
              Comparison ID-{id}
            </Typography>
            <Chip
              style={{
                width: "25% ",
                backgroundColor:
                  input.analyzed === 0
                    ? "#90ee90"
                    : input.analyzed === 1
                    ? "#eec300"
                    : input.analyzed === 2
                    ? "#00FF00"
                    : "#FFFFFF",
              }}
              color="black"
              label={
                input.analyzed === 0
                  ? "Status: Analyze has started successfully for this Comparison"
                  : input.analyzed === 1
                  ? "Status: Analyzing.... Discrepancies Detected will continue to be added"
                  : input.analyzed === 2
                  ? "Status: Completed Analyzing for this Comparison"
                  : ""
              }
              size="large"
            />
            <Divider style={{ marginTop: 16, marginBottom: 16 }} />
            <Paper className={classes.paper}>
              <CardContent style={{ paddingBottom: 16 }}>
                <Container maxWidth={false}>
                  <Grid container spacing={3}>
                    <Grid item lg={12} md={12} xl={12} xs={12}>
                      <FormControl
                        style={{ float: "right" }}
                        className={classes.formControl}
                      >
                        <TextField
                          size="small"
                          onChange={handleSearchChangeBRR}
                          disabled={result.length === 0}
                          id="comparisonSearch"
                          label="Search"
                          variant="outlined"
                          // disabled={true}
                        />
                      </FormControl>
                    </Grid>
                  </Grid>
                </Container>
                <Divider style={{ marginTop: 16, marginBottom: 16 }} />
                <TableContainer>
                  <Table
                    className={classes.table}
                    aria-labelledby="tableTitle"
                    aria-label="enhanced table"
                  >
                    <EnhancedTableHeadBRR
                      classes={classes}
                      order={order}
                      orderBy={orderBy}
                      onRequestSort={handleRequestSort}
                    />
                    {loading ? (
                      <TableBody>
                        <TableRow align="right">
                          <TableCell colSpan={16}>
                            <Loader
                              type="ThreeDots"
                              color="#000000"
                              height={50}
                              width={50}
                              //timeout={10000}
                              style={{ marginLeft: "50%" }}
                            />
                          </TableCell>
                        </TableRow>
                      </TableBody>
                    ) : (
                      <TableBody>
                        {list.length === 0 ? (
                          <TableRow>
                            <TableCell colSpan={16}>
                              No Discrepancy or Issue Found
                            </TableCell>
                          </TableRow>
                        ) : (
                          stableSort(list, getComparator(order, orderBy))
                            .slice(
                              page * rowsPerPage,
                              page * rowsPerPage + rowsPerPage
                            )
                            .map((request) => {
                              const analyzeBRRRow = (
                                <>
                                  <TableCell style={{ width: "10%" }}>
                                    {request.line}
                                  </TableCell>
                                  <TableCell style={{ width: "20%" }}>
                                    {request.employeeId}
                                  </TableCell>
                                  <TableCell style={{ width: "20%" }}>
                                    {request.employmentStatusId}
                                  </TableCell>
                                  <TableCell style={{ width: "10%" }}>
                                    {request.effectiveStart}
                                  </TableCell>
                                  <TableCell style={{ width: "10%" }}>
                                    {request.effectiveEnd}
                                  </TableCell>
                                  <TableCell style={{ width: "10%" }}>
                                    {request.table}
                                  </TableCell>
                                  <TableCell style={{ width: "20%" }}>
                                    {request.discrepancy}
                                  </TableCell>
                                </>
                              );
                              return (
                                <TableRow
                                  hover
                                  tabIndex={-1}
                                  key={request.index}
                                >
                                  {analyzeBRRRow}
                                </TableRow>
                              );
                            })
                        )}
                      </TableBody>
                    )}
                  </Table>
                </TableContainer>
                <TablePagination
                  rowsPerPageOptions={[10, 15, 25]}
                  component="div"
                  count={list.length}
                  rowsPerPage={rowsPerPage}
                  page={page}
                  onChangePage={handleChangePage}
                  onChangeRowsPerPage={handleChangeRowsPerPage}
                />
              </CardContent>
            </Paper>
          </Card>
        </>
      ) : input.job === "JobStepRecalc" ? (
        <>
          {" "}
          <Divider />
          <Divider />
          <Divider style={{ marginTop: 16, marginBottom: 16 }} />
          <Divider />
          <Divider />
          <Card>
            <Divider style={{ marginTop: 16, marginBottom: 16 }} />
            <Typography
              variant="h2"
              align="center"
              color="primary"
              style={{ margin: "2rem" }}
            >
              Analyze
            </Typography>
            <Typography
              variant="h2"
              align="center"
              color="primary"
              style={{ margin: "2rem" }}
            >
              Comparison ID-{id}
            </Typography>
            <Chip
              style={{
                width: "25% ",
                backgroundColor:
                  input.analyzed === 0
                    ? "#90ee90"
                    : input.analyzed === 1
                    ? "#eec300"
                    : input.analyzed === 2
                    ? "#00FF00"
                    : "#FFFFFF",
              }}
              color="black"
              label={
                input.analyzed === 0
                  ? "Status: Analyze has started successfully for this Comparison"
                  : input.analyzed === 1
                  ? "Status: Analyzing.... Discrepancies Detected will continue to be added"
                  : input.analyzed === 2
                  ? "Status: Completed Analyzing for this Comparison"
                  : ""
              }
              size="large"
            />
            <Divider style={{ marginTop: 16, marginBottom: 16 }} />
            <Paper className={classes.paper}>
              <CardContent style={{ paddingBottom: 16 }}>
                <Container maxWidth={false}>
                  <Grid container spacing={3}>
                    <Grid item lg={12} md={12} xl={12} xs={12}>
                      <FormControl
                        style={{ float: "right" }}
                        className={classes.formControl}
                      >
                        <TextField
                          size="small"
                          onChange={handleSearchChangeJSR}
                          disabled={result.length === 0}
                          id="comparisonSearch"
                          label="Search"
                          variant="outlined"
                          // disabled={true}
                        />
                      </FormControl>
                    </Grid>
                  </Grid>
                </Container>
                <Divider style={{ marginTop: 16, marginBottom: 16 }} />
                <TableContainer>
                  <Table
                    className={classes.table}
                    aria-labelledby="tableTitle"
                    aria-label="enhanced table"
                  >
                    <EnhancedTableHeadJSR
                      classes={classes}
                      order={order}
                      orderBy={orderBy}
                      onRequestSort={handleRequestSort}
                    />
                    {loading ? (
                      <TableBody>
                        <TableRow align="right">
                          <TableCell colSpan={16}>
                            <Loader
                              type="ThreeDots"
                              color="#000000"
                              height={50}
                              width={50}
                              //timeout={10000}
                              style={{ marginLeft: "50%" }}
                            />
                          </TableCell>
                        </TableRow>
                      </TableBody>
                    ) : (
                      <TableBody>
                        {list.length === 0 ? (
                          <TableRow>
                            <TableCell colSpan={16}>
                              No Discrepancy or Issue Found
                            </TableCell>
                          </TableRow>
                        ) : (
                          stableSort(list, getComparator(order, orderBy))
                            .slice(
                              page * rowsPerPage,
                              page * rowsPerPage + rowsPerPage
                            )
                            .map((request) => {
                              const analyzeJSRRow = (
                                <>
                                  <TableCell style={{ width: "20%" }}>
                                    {request.line}
                                  </TableCell>
                                  <TableCell style={{ width: "20%" }}>
                                    {request.employeeId}
                                  </TableCell>
                                  <TableCell style={{ width: "20%" }}>
                                    {request.employeeWorkAssignmentId}
                                  </TableCell>
                                  <TableCell style={{ width: "20%" }}>
                                    {request.table}
                                  </TableCell>
                                  <TableCell style={{ width: "20%" }}>
                                    {request.discrepancy}
                                  </TableCell>
                                </>
                              );
                              return (
                                <TableRow
                                  hover
                                  tabIndex={-1}
                                  key={request.index}
                                >
                                  {analyzeJSRRow}
                                </TableRow>
                              );
                            })
                        )}
                      </TableBody>
                    )}
                  </Table>
                </TableContainer>
                <TablePagination
                  rowsPerPageOptions={[10, 15, 25]}
                  component="div"
                  count={list.length}
                  rowsPerPage={rowsPerPage}
                  page={page}
                  onChangePage={handleChangePage}
                  onChangeRowsPerPage={handleChangeRowsPerPage}
                />
              </CardContent>
            </Paper>
          </Card>
        </>
      ) : input.job === "AE_Sample" ? (
        <>
          {" "}
          <Divider />
          <Divider />
          <Divider style={{ marginTop: 16, marginBottom: 16 }} />
          <Divider />
          <Divider />
          <Card>
            <Divider style={{ marginTop: 16, marginBottom: 16 }} />
            <Typography
              variant="h2"
              align="center"
              color="primary"
              style={{ margin: "2rem" }}
            >
              Analyze
            </Typography>
            <Typography
              variant="h2"
              align="center"
              color="primary"
              style={{ margin: "2rem" }}
            >
              Comparison ID-{id}
            </Typography>
            <Chip
              style={{
                width: "25% ",
                backgroundColor:
                  input.analyzed === 0
                    ? "#90ee90"
                    : input.analyzed === 1
                    ? "#eec300"
                    : input.analyzed === 2
                    ? "#00FF00"
                    : "#FFFFFF",
              }}
              color="black"
              label={
                input.analyzed === 0
                  ? "Status: Analyze has started successfully for this Comparison"
                  : input.analyzed === 1
                  ? "Status: Analyzing.... Discrepancies Detected will continue to be added"
                  : input.analyzed === 2
                  ? "Status: Completed Analyzing for this Comparison"
                  : ""
              }
              size="large"
            />
            <Divider style={{ marginTop: 16, marginBottom: 16 }} />
            <Paper className={classes.paper}>
              <CardContent style={{ paddingBottom: 16 }}>
                <Container maxWidth={false}>
                  <Grid container spacing={3}>
                    <Grid item lg={12} md={12} xl={12} xs={12}>
                      <FormControl
                        style={{ float: "right" }}
                        className={classes.formControl}
                      >
                        <TextField
                          size="small"
                          onChange={handleSearchChangeAE}
                          disabled={result.length === 0}
                          id="comparisonSearch"
                          label="Search"
                          variant="outlined"
                          // disabled={true}
                        />
                      </FormControl>
                    </Grid>
                  </Grid>
                </Container>
                <Divider style={{ marginTop: 16, marginBottom: 16 }} />
                <TableContainer>
                  <Table
                    className={classes.table}
                    aria-labelledby="tableTitle"
                    aria-label="enhanced table"
                  >
                    <EnhancedTableHeadAE
                      classes={classes}
                      order={order}
                      orderBy={orderBy}
                      onRequestSort={handleRequestSort}
                    />
                    {loading ? (
                      <TableBody>
                        <TableRow align="right">
                          <TableCell colSpan={16}>
                            <Loader
                              type="ThreeDots"
                              color="#000000"
                              height={50}
                              width={50}
                              //timeout={10000}
                              style={{ marginLeft: "50%" }}
                            />
                          </TableCell>
                        </TableRow>
                      </TableBody>
                    ) : (
                      <TableBody>
                        {list.length === 0 ? (
                          <TableRow>
                            <TableCell colSpan={16}>
                              No Discrepancy or Issue Found
                            </TableCell>
                          </TableRow>
                        ) : (
                          stableSort(list, getComparator(order, orderBy))
                            .slice(
                              page * rowsPerPage,
                              page * rowsPerPage + rowsPerPage
                            )
                            .map((request) => {
                              const analyzeAERow = (
                                <>
                                  <TableCell style={{ width: "20%" }}>
                                    {request.line}
                                  </TableCell>
                                  <TableCell style={{ width: "20%" }}>
                                    {request.employeeId}
                                  </TableCell>
                                  <TableCell style={{ width: "20%" }}>
                                    {request.table}
                                  </TableCell>
                                  <TableCell style={{ width: "20%" }}>
                                    {request.discrepancy}
                                  </TableCell>
                                </>
                              );
                              return (
                                <TableRow
                                  hover
                                  tabIndex={-1}
                                  key={request.index}
                                >
                                  {analyzeAERow}
                                </TableRow>
                              );
                            })
                        )}
                      </TableBody>
                    )}
                  </Table>
                </TableContainer>
                <TablePagination
                  rowsPerPageOptions={[10, 15, 25]}
                  component="div"
                  count={list.length}
                  rowsPerPage={rowsPerPage}
                  page={page}
                  onChangePage={handleChangePage}
                  onChangeRowsPerPage={handleChangeRowsPerPage}
                />
              </CardContent>
            </Paper>
          </Card>
        </>
      ) : input.job === "Export" ? (
        <>
          {" "}
          <Divider />
          <Divider />
          <Divider style={{ marginTop: 16, marginBottom: 16 }} />
          <Divider />
          <Divider />
          <Card>
            <Divider style={{ marginTop: 16, marginBottom: 16 }} />
            <Typography
              variant="h2"
              align="center"
              color="primary"
              style={{ margin: "2rem" }}
            >
              Analyze
            </Typography>
            <Typography
              variant="h2"
              align="center"
              color="primary"
              style={{ margin: "2rem" }}
            >
              Comparison ID-{id}
            </Typography>
            <Chip
              style={{
                width: "25% ",
                backgroundColor:
                  input.analyzed === 0
                    ? "#90ee90"
                    : input.analyzed === 1
                    ? "#eec300"
                    : input.analyzed === 2
                    ? "#00FF00"
                    : "#FFFFFF",
              }}
              color="black"
              label={
                input.analyzed === 0
                  ? "Status: Analyze has started successfully for this Comparison"
                  : input.analyzed === 1
                  ? "Status: Analyzing.... Discrepancies Detected will continue to be added"
                  : input.analyzed === 2
                  ? "Status: Completed Analyzing for this Comparison"
                  : ""
              }
              size="large"
            />
            <Divider style={{ marginTop: 16, marginBottom: 16 }} />
            <Paper className={classes.paper}>
              <CardContent style={{ paddingBottom: 16 }}>
                <Container maxWidth={false}>
                  <Grid container spacing={3}>
                    <Grid item lg={12} md={12} xl={12} xs={12}>
                      <FormControl
                        style={{ float: "right" }}
                        className={classes.formControl}
                      >
                        <TextField
                          size="small"
                          onChange={handleSearchChangeE}
                          disabled={result.length === 0}
                          id="comparisonSearch"
                          label="Search"
                          variant="outlined"
                          // disabled={true}
                        />
                      </FormControl>
                    </Grid>
                  </Grid>
                </Container>
                <Divider style={{ marginTop: 16, marginBottom: 16 }} />
                <TableContainer>
                  <Table
                    className={classes.table}
                    aria-labelledby="tableTitle"
                    aria-label="enhanced table"
                  >
                    <EnhancedTableHeadE
                      classes={classes}
                      order={order}
                      orderBy={orderBy}
                      onRequestSort={handleRequestSort}
                    />
                    {loading ? (
                      <TableBody>
                        <TableRow align="right">
                          <TableCell colSpan={16}>
                            <Loader
                              type="ThreeDots"
                              color="#000000"
                              height={50}
                              width={50}
                              //timeout={10000}
                              style={{ marginLeft: "50%" }}
                            />
                          </TableCell>
                        </TableRow>
                      </TableBody>
                    ) : (
                      <TableBody>
                        {list.length === 0 ? (
                          <TableRow>
                            <TableCell colSpan={16}>
                              No Discrepancy or Issue Found
                            </TableCell>
                          </TableRow>
                        ) : (
                          stableSort(list, getComparator(order, orderBy))
                            .slice(
                              page * rowsPerPage,
                              page * rowsPerPage + rowsPerPage
                            )
                            .map((request) => {
                              const analyzeERow = (
                                <>
                                  <TableCell style={{ width: "20%" }}>
                                    {request.line}
                                  </TableCell>
                                  <TableCell style={{ width: "20%" }}>
                                    {request.discrepancy}
                                  </TableCell>
                                </>
                              );
                              return (
                                <TableRow
                                  hover
                                  tabIndex={-1}
                                  key={request.index}
                                >
                                  {analyzeERow}
                                </TableRow>
                              );
                            })
                        )}
                      </TableBody>
                    )}
                  </Table>
                </TableContainer>
                <TablePagination
                  rowsPerPageOptions={[10, 15, 25]}
                  component="div"
                  count={list.length}
                  rowsPerPage={rowsPerPage}
                  page={page}
                  onChangePage={handleChangePage}
                  onChangeRowsPerPage={handleChangeRowsPerPage}
                />
              </CardContent>
            </Paper>
          </Card>
        </>
      ) : (
        <></>
      )}
    </>
  );
};

export default AnalyzeTable;
