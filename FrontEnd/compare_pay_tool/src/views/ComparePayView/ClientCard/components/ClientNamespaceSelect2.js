import React, { useEffect, useState } from "react";
import {
  Box,
  FormControl,
  Typography,
  Button,
  TextField,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  makeStyles,
} from "@material-ui/core";
import Autocomplete from "@material-ui/lab/Autocomplete";
import axios from "axios";
import Loader from "react-loader-spinner";
import { namespaceAPI } from "src/components/APIBase/BaseURL";
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

const ClientNamespaceSelect2 = ({ ClientInfo, setClientInfo, setSubOpen }) => {
  const classes = useStyles();
  const [list, setList] = useState([]);
  const [loading, setLoading] = useState(false);
  const [displayArr, setDisplayArr] = useState([]);
  const [open, setOpen] = useState(false);

  useEffect(() => {
    if (ClientInfo.clientId > 0 && ClientInfo.id === 1 && ClientInfo.category) {
      setLoading(true);
      let api = async () => {
        let url =
          namespaceAPI + ClientInfo.category + "&id=" + ClientInfo.clientId;
        //gets the list of name spaces
        await axios({
          method: "GET",
          url,
        })
          .then(function (response) {
            setList(response.data);
            console.log("namespaceAPI in NamespaceSelect2 Success");
          })
          .finally(() => {
            setLoading(false);
          })
          .catch(function (error) {
            console.log(error);
            console.log("namespaceAPI in NamespaceSelect2");
          });
      };
      api();
    }
  }, [ClientInfo.category, ClientInfo.clientId, ClientInfo.id]);

  useEffect(() => {
    let tmp = [];
    if (list) {
      list.forEach(function (item) {
        if (
          !item.dbName.includes("[DB not available]") &&
          !item.site.includes("root")
        ) {
          tmp.push(item);
        }
      });
      if (tmp.length > 0) {
        tmp = tmp.sort((a, b) => parseInt(a.version) - parseInt(b.version));
        tmp.forEach(function (x) {
          if (ClientInfo.category.toLowerCase() === "qa") {
            // need to check for wfm, and payroll clients
            const hrpayroll =
              /([a-z])*(hrpayroll)([a-z])*([0-9])+( (- )?[0-9]+)?\w+/g;
            const hr = /([a-z])*(hr)([a-z])*([0-9])+( (- )?[0-9]+)?\w+/g;
            const payroll =
              /([a-z])*(payroll)([a-z])*([0-9])+( (- )?[0-9]+)?\w+/g;
            const wfm = /([a-z])*(wfm)([a-z])*([0-9])+( (- )?[0-9]+)?\w+/g;
            let site = "";
            if (hrpayroll.test(ClientInfo.clientName.toLowerCase())) {
              site = "qa";
            } else if (hr.test(ClientInfo.clientName.toLowerCase())) {
              site = "qahr";
            } else if (payroll.test(ClientInfo.clientName.toLowerCase())) {
              site = "payroll";
            } else if (wfm.test(ClientInfo.clientName.toLowerCase())) {
              site = "qawfm";
            } else {
              site = "qa";
            }
            x.label = x.version + site + ClientInfo.clientId;
          } else {
            x.label = x.dbName.toLowerCase();
          }
        });
        setDisplayArr(tmp);
        setOpen(false);
      } else {
        setDisplayArr(tmp);
        setLoading(true);
        setOpen(true);
      }
    }
  }, [list, ClientInfo.category, ClientInfo.clientId, ClientInfo.clientName]);

  const handleClose = () => {
    setOpen(false);
  };

  const switchToolbox = () => {
    setClientInfo((prev) => {
      let update = { ...prev };
      update.id = 3;
      update.envName = "toolbox";
      update.filePath = "";
      update.category = "";
      update.status = 0;
      update.clientId = -1;
      update.clientName = "";
      update.clientDb = "";
      update.site = "";
      update.dbSize = 0;
      update.dbSize2 = 0;
      update.adminDb = "";
      update.adminDbSrv = "";
      update.namespace = null;
      update.namespace2 = null;
      return update;
    });

    setOpen(false);
  };

  if (loading) {
    return (
      <Box mb={2}>
        <div style={{ justifyContent: "center", marginLeft: "1em" }}>
          <Loader type="ThreeDots" color="#000000" height={50} width={50} />
        </div>
      </Box>
    );
  } else {
    return (
      <Box mb={2}>
        <FormControl required className={classes.formControl}>
          {displayArr.length > 0 ? (
            <Autocomplete
              id="clientNamespace2"
              options={displayArr}
              required
              value={ClientInfo.namespace2}
              onChange={(event, newValue) => {
                setSubOpen(false);
                setClientInfo((prev) => {
                  let update = { ...prev };
                  update.namespace2 = newValue;
                  update.dbSize2 = newValue ? newValue.dbSize : 0;
                  return update;
                });
              }}
              getOptionSelected={(option, value) => option.site === value.site}
              getOptionLabel={(option) =>
                option.label
                  ? `${option.site?.toLowerCase()} ${option.dbName?.toLowerCase()} [${
                      option.version
                    }]`
                  : ""
              }
              renderInput={(params) => (
                <TextField
                  {...params}
                  label="Client DB Version 2"
                  required
                  variant="outlined"
                />
              )}
            />
          ) : (
            <></>
          )}
          <Dialog
            open={false}
            onClose={handleClose}
            aria-labelledby="emptyNamespaceTitle"
            aria-describedby="emptyNamespaceContent"
          >
            <DialogTitle id="emptyNamespaceTitle">
              <Typography style={{ fontSize: "1.25rem" }}>
                Client Empty Namespace
              </Typography>
            </DialogTitle>
            <DialogContent>
              <DialogContentText id="emptyNamespaceContent">
                The selected Client has no existing database namespace versions
                in our admin database servers. Consider restoring the database
                from Toolbox servers instead.
              </DialogContentText>
            </DialogContent>
            <DialogActions>
              <Button onClick={handleClose} color="primary">
                Change Client
              </Button>
              <Button onClick={switchToolbox} color="primary">
                Switch to Toolbox
              </Button>
            </DialogActions>
          </Dialog>
          <Typography
            style={{ visibility: open ? "visible" : "hidden", color: "red" }}
            variant="subtitle1"
            gutterBottom
          >
            The selected Client has no existing database namespace versions in
            our admin database servers
          </Typography>
        </FormControl>
      </Box>
    );
  }
};

export default ClientNamespaceSelect2;
