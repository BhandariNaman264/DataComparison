import React, { useState, useEffect } from "react";
import PropTypes from "prop-types";
import axios from "axios";
import {
  Card,
  Container,
  Grid,
  CardContent,
  Chip,
  Divider,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Typography,
  TableSortLabel,
  FormControl,
  Button,
  InputLabel,
  Select,
  MenuItem,
  makeStyles,
} from "@material-ui/core";
import TableContainer from "@material-ui/core/TableContainer";
import TablePagination from "@material-ui/core/TablePagination";
import Paper from "@material-ui/core/Paper";
import FormControlLabel from "@material-ui/core/FormControlLabel";
import { dates } from "src/util/DateTime/dates";
import { inputAPI } from "src/components/APIBase/BaseURL";
import Switch from "@material-ui/core/Switch";
import DateFnsUtils from "@date-io/date-fns";
import {
  MuiPickersUtilsProvider,
  KeyboardDatePicker,
} from "@material-ui/pickers";
import Loader from "react-loader-spinner";
import "react-loader-spinner/dist/loader/css/react-spinner-loader.css";
import { Link } from "react-router-dom";

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

const headCells = [
  { id: "user_name", numeric: false, label: "User" },
  { id: "date_time", numeric: false, label: "Initiated Date-Time" },
  { id: "task_name", numeric: false, label: "Task Name" },
  { id: "job", numeric: false, label: "Job" },
  { id: "job_id", numeric: false, label: "ID" },
  { id: "start_time", numeric: false, label: "Start Date" },
  { id: "start_end", numeric: false, label: "End Date" },
  { id: "client", numeric: false, label: "Client" },
  { id: "dbname1", numeric: false, label: "Version 1" },
  { id: "dbname2", numeric: false, label: "Version 2" },
  { id: "org", numeric: false, label: "Org" },
  { id: "status1", numeric: false, label: "Version 1 Status" },
  { id: "status2", numeric: false, label: "Version 2 Status" },
  { id: "results", numeric: false, label: "Comparison Status" },
  { id: "analyze", numeric: false, label: "" },
];

function EnhancedTableHead(props) {
  const { classes, order, orderBy, onRequestSort } = props;
  const createSortHandler = (property) => (event) => {
    onRequestSort(event, property);
  };

  return (
    <TableHead>
      <TableRow>
        {headCells.map((headCell) => (
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

EnhancedTableHead.propTypes = {
  classes: PropTypes.object.isRequired,
  order: PropTypes.oneOf(["asc", "desc"]).isRequired,
  orderBy: PropTypes.string.isRequired,
};

const ResultTable = ({ userInfo }) => {
  const classes = useStyles();

  const [result, setresult] = useState([]);
  const [list, setlist] = useState([]);

  const [searchParam, setSearchParam] = useState("All Comparison Results");

  const [order, setOrder] = useState("asc");
  const [orderBy, setOrderBy] = useState("restoreRequestBy");

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(10);

  const [dense, setDense] = useState(false);

  const [loading, setLoading] = useState(true);

  const [filter, setFilter] = useState("all");

  const [jobfilter, setjobFilter] = useState("all");

  const day = new Date();

  const prevDay = new Date(day);
  prevDay.setDate(prevDay.getDate() - 1);

  const today = `${dates.pad(day.getMonth() + 1, 2)}/${dates.pad(
    day.getDate(),
    2
  )}/${day.getFullYear()}`;

  const yesterday = `${dates.pad(prevDay.getMonth() + 1, 2)}/${dates.pad(
    prevDay.getDate(),
    2
  )}/${prevDay.getFullYear()}`;

  const [selectedDate, setSelectedDate] = useState(day);

  function searchFor(toSearch) {
    var results = [];

    toSearch = trimString(toSearch.toLowerCase());

    for (var i = 0; i < result.length; i++) {
      if (
        result[i].user_Name?.toLowerCase().includes(toSearch) ||
        result[i].task_Name?.toLowerCase().includes(toSearch) ||
        result[i].id?.toString().includes(toSearch) ||
        result[i].client?.toLowerCase().includes(toSearch) ||
        result[i].job?.toLowerCase().includes(toSearch) ||
        result[i].dbName_1?.toLowerCase().includes(toSearch) ||
        result[i].dbName_2?.toLowerCase().includes(toSearch) ||
        result[i].org?.toLowerCase().includes(toSearch)
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

  const handleSearchChange = (event) => {
    if (event.target.value === "") {
      setlist(result);
    } else {
      setlist(searchFor(event.target.value));
    }
    setPage(0);
  };

  const handleDateChange = (date) => {
    setLoading(true);
    setSelectedDate(date);
    setSearchParam("Comparison Results - " + date.toDateString());

    var d = new Date(date);
    var da = d.toDateString();

    // turn the date into the string format MM/dd/yyyy
    if (dates.compare(da, prevDay.toDateString()) === 0) {
      setFilter("yesterday");
      setSearchParam("Comparison Results - Yesterday");
    } else if (dates.compare(da, day.toDateString()) === 0) {
      setFilter("today");
      setSearchParam("Comparison Results - Today");
    } else {
      setFilter("");
    }

    // document.getElementById("requestSearch").value = "";

    var date_filter = `${dates.pad(d.getMonth() + 1, 2)}/${dates.pad(
      d.getDate(),
      2
    )}/${d.getFullYear()}`;

    setlist([]);

    for (let i = 0; i < result.length; i++) {
      setlist(result.filter((item) => item.date.split(",")[0] === date_filter));
    }

    setLoading(false);
  };

  const handleSelectChange = (event) => {
    setLoading(true);
    setFilter(event.target.value);

    switch (event.target.value) {
      case "all":
        setSearchParam("All Comparison Results");
        setSelectedDate(day);

        setlist([]);

        setlist(result);

        break;

      case "my":
        setSearchParam("My Comparison Results");
        setSelectedDate(day);

        setlist([]);
        for (let i = 0; i < result.length; i++) {
          setlist(
            result.filter((item) => item.user_ID === userInfo.samAccountName)
          );
        }

        break;

      case "today":
        setSearchParam("Comparison Results - Today");
        setSelectedDate(day);

        setlist([]);
        for (let i = 0; i < result.length; i++) {
          setlist(result.filter((item) => item.date.split(",")[0] === today));
        }

        break;
      case "incomplete":
        setSearchParam("Comparison Results - Incomplete");
        setSelectedDate(day);

        setlist([]);
        for (let i = 0; i < result.length; i++) {
          setlist(
            result.filter(
              (item) =>
                item.results === "Comparison Not Started" ||
                item.results === "Comparison has started"
            )
          );
        }

        break;
      case "failed":
        setSearchParam("Comparison Results - Failed");
        setSelectedDate(day);

        setlist([]);
        for (let i = 0; i < result.length; i++) {
          setlist(
            result.filter(
              (item) =>
                item.results === "No Results-Job Failed" ||
                item.results === "No Results-Queue Failed"
            )
          );
        }

        break;
      case "complete":
        setSearchParam("Comparison Results - Completed");
        setSelectedDate(day);

        setlist([]);
        for (let i = 0; i < result.length; i++) {
          setlist(result.filter((item) => item.compared === 2));
        }

        break;
      case "yesterday":
        setSearchParam("Comparison Results - Yesterday");
        setSelectedDate(prevDay);

        setlist([]);
        for (let i = 0; i < result.length; i++) {
          setlist(
            result.filter((item) => item.date.split(",")[0] === yesterday)
          );
        }

        break;
      default:
        break;
    }

    setLoading(false);

    // document.getElementById("requestSearch").value = "";
  };

  const handleJobSelectChange = (event) => {
    setLoading(true);
    setFilter(event.target.value);

    switch (event.target.value) {
      case "all":
        setjobFilter("all");

        setlist([]);

        setlist(result);

        break;

      case "PSR":
        setjobFilter("PSR");
        setlist([]);

        for (let i = 0; i < result.length; i++) {
          setlist(result.filter((item) => item.job === "PSR"));
        }

        break;

      case "BRR":
        setjobFilter("BRR");
        setlist([]);

        for (let i = 0; i < result.length; i++) {
          setlist(result.filter((item) => item.job === "BRR"));
        }

        break;
      case "JobStepRecalc":
        setjobFilter("JobStepRecalc");
        setlist([]);

        for (let i = 0; i < result.length; i++) {
          setlist(result.filter((item) => item.job === "JobStepRecalc"));
        }

        break;
      case "SCR":
        setjobFilter("SCR");
        setlist([]);

        for (let i = 0; i < result.length; i++) {
          setlist(result.filter((item) => item.job === "SCR"));
        }

        break;
      case "AE_Sample":
        setjobFilter("AE_Sample");
        setlist([]);

        for (let i = 0; i < result.length; i++) {
          setlist(result.filter((item) => item.job === "AE_Sample"));
        }

        break;
      case "Export":
        setjobFilter("Export");
        setlist([]);

        for (let i = 0; i < result.length; i++) {
          setlist(result.filter((item) => item.job === "Export"));
        }

        break;
      default:
        break;
    }

    setLoading(false);

    // document.getElementById("requestSearch").value = "";
  };

  useEffect(() => {
    setLoading(true);
    let resultApi = async () => {
      let url = inputAPI;
      await axios({
        method: "GET",
        url,
      })
        .then(function (response) {
          setresult(response.data);
          setlist(response.data);
          setLoading(false);
        })
        .catch(function (error) {
          console.log(error);
        });
    };
    resultApi();
  }, []);

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

  const handleChangeDense = (event) => {
    setDense(event.target.checked);
  };

  return (
    <Card>
      <Typography
        variant="h2"
        align="center"
        color="primary"
        style={{ margin: "2rem" }}
      >
        {searchParam}
      </Typography>
      <Typography
        variant="h3"
        align="center"
        color="primary"
        style={{ margin: "2rem" }}
      >
        *Use filter to quickly search for Comparison Results you are interested
        in*
      </Typography>
      <Divider style={{ marginTop: 16, marginBottom: 16 }} />
      <Paper className={classes.paper}>
        <CardContent style={{ paddingBottom: 16 }}>
          <Container maxWidth={false}>
            <Grid container spacing={3}>
              <Grid item lg={4} md={4} xl={4} xs={12}>
                <FormControl style={{ float: "left" }}>
                  <MuiPickersUtilsProvider utils={DateFnsUtils}>
                    <Grid container justify="space-around">
                      <KeyboardDatePicker
                        disableToolbar
                        variant="inline"
                        format="MM/dd/yyyy"
                        margin="normal"
                        id="comparisonDateFilter"
                        disabled={result.length === 0}
                        disableFuture={true}
                        label="Filter by Comparison Initiated Date"
                        value={selectedDate}
                        onChange={handleDateChange}
                        KeyboardButtonProps={{
                          "aria-label": "change date",
                        }}
                        style={{ minWidth: 350 }}
                      />
                    </Grid>
                  </MuiPickersUtilsProvider>
                </FormControl>
              </Grid>
              <Grid item lg={4} md={4} xl={4} xs={12}>
                <FormControl
                  style={{ float: "center" }}
                  className={classes.formControl}
                >
                  <InputLabel id="jobFilterLabel">Job Filter</InputLabel>
                  <Select
                    labelId="jobFilterLabel"
                    id="jobFilterSelect"
                    value={jobfilter}
                    disabled={result.length === 0}
                    onChange={handleJobSelectChange}
                    label="Job Filter"
                  >
                    <MenuItem disabled value="">
                      <em>--View--</em>
                    </MenuItem>
                    <MenuItem value="all">All Jobs</MenuItem>
                    <MenuItem value="PSR">Pay Summary Recalc</MenuItem>
                    <MenuItem value="BRR">Base Rate Recalc</MenuItem>
                    <MenuItem value="SCR">Schedule Cost Recalc</MenuItem>
                    <MenuItem value="JobStepRecalc">Job Step Recalc</MenuItem>
                    <MenuItem value="Export">Pay Export</MenuItem>
                    <MenuItem value="AE_Sample">Award Entitlement</MenuItem>
                  </Select>
                </FormControl>
              </Grid>
              <Grid item lg={4} md={4} xl={4} xs={12}>
                <FormControl
                  style={{ float: "right" }}
                  className={classes.formControl}
                >
                  <InputLabel id="comparisonFilterLabel">
                    Comparison Filter
                  </InputLabel>
                  <Select
                    labelId="comparisonFilterLabel"
                    id="comparisonFilterSelect"
                    value={filter}
                    disabled={result.length === 0}
                    onChange={handleSelectChange}
                    label="Comparison Filter"
                  >
                    <MenuItem disabled value="">
                      <em>--View--</em>
                    </MenuItem>
                    <MenuItem value="all">All Comparisons</MenuItem>
                    <MenuItem value="my">My Comparisons</MenuItem>
                    <MenuItem value="today">
                      Comparisons Initiated Today
                    </MenuItem>
                    <MenuItem value="yesterday">
                      Comparisons Initiated Yesterday
                    </MenuItem>
                    <MenuItem value="complete">
                      Comparison Results - Completed
                    </MenuItem>
                    <MenuItem value="incomplete">
                      Comparison Results - Incomplete
                    </MenuItem>
                    <MenuItem value="failed">
                      Comparison Results - Failed
                    </MenuItem>
                  </Select>
                </FormControl>
              </Grid>
            </Grid>
          </Container>
          <Divider style={{ marginTop: 16, marginBottom: 16 }} />
          <Container maxWidth={false}>
            <Grid container spacing={3}>
              <Grid item lg={12} md={12} xl={12} xs={12}>
                <FormControl
                  style={{ float: "right" }}
                  className={classes.formControl}
                >
                  <TextField
                    size="small"
                    onChange={handleSearchChange}
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
          <TableContainer>
            <Table
              className={classes.table}
              aria-labelledby="tableTitle"
              aria-label="enhanced table"
            >
              <EnhancedTableHead
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
                      <TableCell colSpan={16}>No Comparison Found</TableCell>
                    </TableRow>
                  ) : (
                    stableSort(list, getComparator(order, orderBy))
                      .slice(
                        page * rowsPerPage,
                        page * rowsPerPage + rowsPerPage
                      )
                      .map((request) => {
                        let label1;
                        let color1;

                        if (request.forceCompareOnly === true) {
                          label1 = "Force Compare Enabled";
                          color1 = "#0000FF";
                        } else if (request.status1 === "") {
                          label1 = "Not Started";
                          color1 = "#D6D5CB";
                        } else if (request.status1 === "JobQueued") {
                          label1 = "Queued";
                          color1 = "#eec300";
                        } else if (request.status1 === "JobQueueFailed") {
                          label1 = "Queued Failed";
                          color1 = "#ffcccb";
                        } else if (request.status1 === "JobInProgress") {
                          label1 = "In Progess";
                          color1 = "#90EE90";
                        } else if (request.status1 === "JobCompleted") {
                          label1 = "Completed";
                          color1 = "#00FF00";
                        } else if (request.status1 === "JobFailed") {
                          label1 = "Failed";
                          color1 = "#FF0000";
                        }

                        let label2;
                        let color2;

                        if (request.forceCompareOnly === true) {
                          label2 = "Force Compare Enabled";
                          color2 = "#0000FF";
                        } else if (request.status2 === "") {
                          label2 = "Not Started";
                          color2 = "#D6D5CB";
                        } else if (request.status2 === "JobQueued") {
                          label2 = "Queued";
                          color2 = "#eec300";
                        } else if (request.status2 === "JobQueueFailed") {
                          label1 = "Queued Failed";
                          color1 = "#ffcccb";
                        } else if (request.status2 === "JobInProgress") {
                          label2 = "In Progess";
                          color2 = "#90EE90";
                        } else if (request.status2 === "JobCompleted") {
                          label2 = "Completed";
                          color2 = "#00FF00";
                        } else if (request.status2 === "JobFailed") {
                          label2 = "Failed";
                          color2 = "#FF0000";
                        }

                        let labelcomparison;
                        let colorcomparison;

                        if (request.results === "WARNING") {
                          labelcomparison = "With Difference";
                          colorcomparison = "#FF0000";
                        } else if (request.results === "SUCCESS") {
                          labelcomparison = "No Difference";
                          colorcomparison = "#00FF00";
                        } else if (
                          request.status1 === "JobCompleted" &&
                          request.status2 === "JobCompleted" &&
                          request.results === ""
                        ) {
                          labelcomparison = "Running Comparison";
                          colorcomparison = "#eec300";
                        } else if (
                          (request.compared =
                            1 && request.forceCompareOnly === true)
                        ) {
                          labelcomparison = "Running Comparison";
                          colorcomparison = "#eec300";
                        } else if (
                          request.results === "No Results-Job Failed" ||
                          request.results === "No Results-Queue Failed"
                        ) {
                          labelcomparison =
                            "Can't Compare - Atleast 1 Version Failed";
                          colorcomparison = "#ffcccb";
                        } else if (request.results === "MANUAL COMPARISON") {
                          labelcomparison = "Manual Comparison";
                          colorcomparison = "#90EE90";
                        } else {
                          labelcomparison = "No Results";
                          colorcomparison = "#D6D5CB";
                        }

                        let joblabel;
                        let jobcolor;

                        if (request.job === "PSR") {
                          joblabel = "PSR";
                          jobcolor = "#964B00";
                        } else if (request.job === "BRR") {
                          joblabel = "BRR";
                          jobcolor = "#4B0082";
                        } else if (request.job === "SCR") {
                          joblabel = "SCR";
                          jobcolor = "#FFA500";
                        } else if (request.job === "Export") {
                          joblabel = "Export";
                          jobcolor = "#0000FF";
                        } else if (request.job === "JobStepRecalc") {
                          joblabel = "JSR";
                          jobcolor = "#00FF00";
                        } else if (request.job === "AE_Sample") {
                          joblabel = "AE";
                          jobcolor = "#8F00FF";
                        }

                        let start_dt;

                        if (request.start_Time === "") {
                          start_dt = " N.A. ";
                        } else {
                          start_dt = request.start_Time.split("T")[0];
                        }

                        let end_dt;

                        if (request.end_Time === "") {
                          end_dt = " N.A. ";
                        } else {
                          end_dt = request.end_Time.split("T")[0];
                        }

                        let org = request.org;

                        if (request.org === "") {
                          org = "Whole Organization";
                        }

                        const resultRow = (
                          <>
                            <TableCell style={{ width: "10%" }}>
                              {request.user_Name}
                            </TableCell>
                            <TableCell style={{ width: "15%" }}>
                              {request.date}
                            </TableCell>
                            <TableCell style={{ width: "10%" }}>
                              {request.task_Name}
                            </TableCell>
                            <TableCell style={{ width: "15%" }} align="center">
                              <Chip
                                style={{
                                  width: "100% ",
                                  backgroundColor: jobcolor,
                                }}
                                color="primary"
                                label={joblabel}
                                size="small"
                              />
                            </TableCell>
                            <TableCell style={{ width: "4%" }}>
                              {request.id}
                            </TableCell>
                            <TableCell style={{ width: "23%" }}>
                              {start_dt}
                            </TableCell>
                            <TableCell style={{ width: "23%" }}>
                              {end_dt}
                            </TableCell>
                            <TableCell style={{ width: "10%" }}>
                              {request.client}
                            </TableCell>
                            <TableCell style={{ width: "10%" }}>
                              {request.dbName_1}
                            </TableCell>
                            <TableCell style={{ width: "10%" }}>
                              {request.dbName_2}
                            </TableCell>
                            <TableCell style={{ width: "10%" }}>
                              {org}
                            </TableCell>
                            <TableCell style={{ width: "15%" }} align="center">
                              <Chip
                                style={{
                                  width: "100% ",
                                  backgroundColor: color1,
                                }}
                                color="primary"
                                label={label1}
                                size="small"
                              />
                            </TableCell>
                            <TableCell style={{ width: "15%" }} align="center">
                              <Chip
                                style={{
                                  width: "100% ",
                                  backgroundColor: color2,
                                }}
                                color="primary"
                                label={label2}
                                size="small"
                              />
                            </TableCell>
                            <TableCell style={{ width: "15%" }} align="center">
                              <Chip
                                style={{
                                  width: "100% ",
                                  backgroundColor: colorcomparison,
                                }}
                                color="primary"
                                label={labelcomparison}
                                size="small"
                              />
                            </TableCell>
                            {request.analyzed !== 0 ? (
                              <TableCell style={{ width: "10%" }}>
                                <Link to={`/analyze/${request.id}`}>
                                  <Button
                                    disabled={request.analyzed === 0}
                                    id="analyze"
                                    variant="contained"
                                    color="primary"
                                    // onClick={() => {}}
                                  >
                                    Analyze
                                  </Button>
                                </Link>
                              </TableCell>
                            ) : (
                              <TableCell style={{ width: "10%" }}>
                                <Button
                                  disabled={request.analyzed === 0}
                                  id="analyze"
                                  variant="contained"
                                  color="primary"
                                  // onClick={() => {}}
                                >
                                  Analyze
                                </Button>
                              </TableCell>
                            )}
                          </>
                        );

                        return (
                          <TableRow hover tabIndex={-1} key={request.index}>
                            {resultRow}
                          </TableRow>
                        );
                      })
                  )}
                </TableBody>
              )}
            </Table>
          </TableContainer>
          <TablePagination
            rowsPerPageOptions={[10, 25, 50]}
            component="div"
            count={list.length}
            rowsPerPage={rowsPerPage}
            page={page}
            onChangePage={handleChangePage}
            onChangeRowsPerPage={handleChangeRowsPerPage}
          />
        </CardContent>
        <FormControlLabel
          style={{
            float: "left",
            marginLeft: 4,
            marginBottom: 16,
            marginTop: 16,
          }}
          control={<Switch checked={dense} onChange={handleChangeDense} />}
          label="Dense padding"
        />
      </Paper>
    </Card>
  );
};

export default ResultTable;
