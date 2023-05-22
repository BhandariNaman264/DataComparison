import React, { useState, useEffect } from "react";
import PropTypes from "prop-types";
import {
  Card,
  CardContent,
  Chip,
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
import Paper from "@material-ui/core/Paper";
import FormControlLabel from "@material-ui/core/FormControlLabel";
import Switch from "@material-ui/core/Switch";
import Loader from "react-loader-spinner";
import "react-loader-spinner/dist/loader/css/react-spinner-loader.css";
import axios from "axios";
import { inputAPI } from "src/components/APIBase/BaseURL";
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
  { id: "again", numeric: false, label: "" },
  { id: "delete", numeric: false, label: "" },
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

const MyComparisonTable = ({ userInfo, result, loading }) => {
  const classes = useStyles();

  const [order, setOrder] = useState("asc");
  const [orderBy, setOrderBy] = useState("comparisonRequestBy");

  const [page, setPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(5);

  const [dense, setDense] = useState(false);

  const [list, setlist] = useState([]);

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

  const refreshPage = () => {
    window.location.reload(true);
  };

  const remove = async (id) => {
    // Delete this Job ID from Results, API and Table, after getting Results and required status.
    await axios({
      method: "DELETE",
      url: inputAPI + id.toString(),
    })
      .then(() => {
        console.log("Job " + id.toString() + " Deleted");
      })
      .catch((err) => {
        console.log("Job " + id.toString() + "was not deleted", err);
      });
  };

  const runAgain = async (request) => {
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
        User_ID: request.user_ID,
        User_Name: request.user_Name,
        User_Email: request.user_Email,
        Client: request.client,
        DBName_1: request.dbName_1,
        DBName_2: request.dbName_2,
        ControlDB_1: request.controlDB_1,
        ControlDB_2: request.controlDB_2,
        ControlDBServer_1: request.controlDBServer_1,
        ControlDBServer_2: request.controlDBServer_2,
        ControlDBServer_Server1: request.controlDBServer_Server1,
        ControlDBServer_Server2: request.controlDBServer_Server2,
        Task_Name: request.task_Name,
        ForceCompareOnly: request.forceCompareOnly,
        RunTask_1: request.runTask_1,
        RunTask_2: request.runTask_2,
        Start_Time: request.start_Time,
        End_Time: request.end_Time,
        Date_Relative_To_Today: request.date_Relative_To_Today,
        Org: request.org,
        Policy: request.policy,
        Pay_Group_Calendar_Id: request.pay_Group_Calendar_Id,
        Export_Mode: request.export_Mode,
        Mock_Transmit: request.mock_Transmit,
        Export_File_Name: request.export_File_Name,
        Job: request.job,
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
          request.job +
            "-Input POST Request is Succesfull and Input is added to Input Table"
        );
      })
      .catch((err) => {
        console.log("Error in " + request.job + " POST Request", err);
      });
  };

  useEffect(() => {
    if (result.length > 0) {
      for (let i = 0; i < result.length; i++) {
        setlist(
          result.filter((item) => item.user_ID === userInfo.samAccountName)
        );
      }
    } else {
      setlist([]);
    }
  }, [userInfo.samAccountName, result]);

  return (
    <Card>
      <Typography
        variant="h2"
        align="center"
        color="primary"
        style={{ margin: "2rem" }}
      >
        My Comparison Results
      </Typography>
      <Divider style={{ marginTop: 16, marginBottom: 16 }} />
      <Paper className={classes.paper}>
        <CardContent style={{ paddingBottom: 16 }}>
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
                            <TableCell style={{ width: "14%" }}>
                              {request.date}
                            </TableCell>
                            <TableCell style={{ width: "10%" }}>
                              {request.task_Name}
                            </TableCell>
                            <TableCell style={{ width: "10%" }} align="center">
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
                            <TableCell style={{ width: "5%" }}>
                              {request.dbName_1}
                            </TableCell>
                            <TableCell style={{ width: "5%" }}>
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
                            <TableCell style={{ width: "10%" }}>
                              <Button
                                disabled={
                                  request.results !== "No Results-Job Failed" &&
                                  request.results !==
                                    "No Results-Queue Failed" &&
                                  request.analyzed !== 2 &&
                                  request.results !== "MANUAL COMPARISON"
                                }
                                id="again"
                                variant="contained"
                                color="primary"
                                onClick={() => {
                                  if (request.analyzed === 2) {
                                    runAgain(request);
                                  } else {
                                    runAgain(request);
                                    remove(request.id);
                                  }

                                  refreshPage();
                                }}
                              >
                                RERUN
                              </Button>
                            </TableCell>
                            <TableCell style={{ width: "10%" }}>
                              <Button
                                disabled={
                                  request.results !== "No Results-Job Failed" &&
                                  request.results !==
                                    "No Results-Queue Failed" &&
                                  request.analyzed !== 2 &&
                                  request.results !== "MANUAL COMPARISON"
                                }
                                id="delete"
                                variant="contained"
                                color="primary"
                                onClick={() => {
                                  remove(request.id);
                                  refreshPage();
                                }}
                              >
                                DELETE
                              </Button>
                            </TableCell>
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
            rowsPerPageOptions={[5, 10, 25]}
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

export default MyComparisonTable;
