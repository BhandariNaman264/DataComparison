import React, { useEffect, useState } from "react";
import {
  Box,
  FormControl,
  TextField,
  FormControlLabel,
  RadioGroup,
  Radio,
  makeStyles,
} from "@material-ui/core";
import Autocomplete from "@material-ui/lab/Autocomplete";
import axios from "axios";
import Loader from "react-loader-spinner";
import { adminClientAPI } from "src/components/APIBase/BaseURL";
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

const ClientSelect = ({
  clientList,
  ClientInfo,
  setClientInfo,
  valid,
  setSubOpen,
}) => {
  const classes = useStyles();
  const [list, setList] = useState([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    let api = async () => {
      switch (ClientInfo.id) {
        case 1:
          if (ClientInfo.category) {
            setLoading(true);
            let url = adminClientAPI + ClientInfo.category;
            //gets the list of clients using the category selected
            await axios({
              method: "GET",
              url,
            })
              .then(function (response) {
                setList(response.data);
                // setInput1ClientList(response.data);
                console.log("adminClientAPI in ClientSelect Success");
              })
              .catch(function (error) {
                console.log(error);
                console.log("adminClientAPI in ClientSelect");
              })
              .finally(() => {
                setLoading(false);
              });
          }
          break;
        default:
          break;
      }
    };
    api();
  }, [ClientInfo.category, ClientInfo.id, clientList, valid]);

  //handles sorting by client ID or by client name (radio buttons)
  function handleSort(sortParam) {
    if (sortParam === "ID") {
      setList(list.sort((a, b) => a.clientId - b.clientId));
    } else {
      setList(
        list.sort((a, b) =>
          a.clientName.trim().localeCompare(b.clientName.trim())
        )
      );
    }
  }

  if (loading || clientList.length < 2) {
    return (
      <Box mb={2}>
        <div style={{ justifyContent: "center", marginLeft: "1em" }}>
          <Loader
            type="ThreeDots"
            color="#000000"
            height={50}
            width={50}
            timeout={20000}
          />
        </div>
      </Box>
    );
  } else {
    return (
      <Box mt={4} mb={2}>
        <FormControl required className={classes.formControl}>
          <RadioGroup
            className={classes.sortGroup}
            row
            aria-label="sort"
            name="sort"
            onChange={(event) => handleSort(event.target.value)}
            defaultValue="Name"
          >
            <FormControlLabel
              value="ID"
              control={<Radio color="primary" />}
              label="Sort Client ID"
              labelPlacement="start"
            />
            <FormControlLabel
              value="Name"
              control={<Radio color="primary" />}
              label="Sort Client Name"
              labelPlacement="start"
            />
          </RadioGroup>
          <Autocomplete
            id="Client"
            options={list}
            required
            value={ClientInfo}
            onChange={(event, newValue) => {
              setSubOpen(false);
              //if the user decides to change the client, all client data previously fetched gets overwritten
              setClientInfo((prev) => {
                let update = { ...prev };
                update.clientId = newValue ? newValue.clientId : -1;
                update.clientName = newValue ? newValue.clientName : "";
                update.clientDb = newValue ? newValue.clientDb : "";
                update.adminDb = newValue ? newValue.adminDb : "";
                update.adminDbSrv = newValue ? newValue.adminDbSrv : "";
                update.site = newValue ? newValue.site : "";
                update.dbSize =
                  newValue && (ClientInfo.id === 2 || ClientInfo.id === 3)
                    ? newValue.dbSize
                    : 0;
                update.namespace = null;
                update.namespace2 = null;
                return update;
              });
            }}
            getOptionSelected={(option, value) =>
              option.clientId === value.clientId &&
              option.clientName === value.clientName &&
              option.adminDb === value.adminDb &&
              option.adminDbSrv === value.adminDbSrv &&
              (ClientInfo.id === 1 ||
                (ClientInfo.id > 1 && option.site === value.site))
            }
            getOptionLabel={(option) =>
              option.clientName
                ? ClientInfo.id < 3
                  ? option.clientName?.toLowerCase() + "-" + option.clientId
                  : option.clientDb === "" ||
                    option.clientDb
                      ?.toLowerCase()
                      .localeCompare(option.clientName?.toLowerCase()) === 0
                  ? option.clientName?.toLowerCase() +
                    "-" +
                    option.clientId +
                    " [" +
                    option.site?.toLowerCase() +
                    "]"
                  : option.clientName?.toLowerCase() +
                    "-" +
                    option.clientId +
                    " (" +
                    option.clientDb?.toLowerCase() +
                    ") [" +
                    option.site?.toLowerCase() +
                    "]"
                : ""
            }
            renderInput={(params) => (
              <TextField
                {...params}
                label={
                  ClientInfo.id === 2 ? "Client Given File Info" : "Client Info"
                }
                required
                variant="outlined"
              />
            )}
          />
          {ClientInfo.site && ClientInfo.id === 3 ? (
            <p
              id="clientDBVersion"
              style={{
                color: "rgba(0, 0, 0, 0.54)",
                marginBottom: "0px",
                marginTop: "5px",
                fontSize: "14px",
                textAlign: "left",
              }}
            >
              Database Version: {ClientInfo.adminDb}
            </p>
          ) : (
            <></>
          )}
        </FormControl>
      </Box>
    );
  }
};

export default ClientSelect;
