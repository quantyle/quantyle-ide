import React from "react";
import PropTypes from "prop-types";
import editorStyle from "../../variables/styles/editorStyle.jsx";
import { Component } from "react";
import {
    withStyles,
} from "@material-ui/core";
import {
    CustomizedTabs,
    ListItem,
    Button,
    FileModal,
} from '../../components';
import {
    autoComplete,
    iconsUrl,
} from "../../variables/global";
import {
    Code,
} from "@material-ui/icons";
import api from "../../providers/api";
import AceEditor from "react-ace";
import "ace-builds/src-noconflict/mode-whale-dojo";
import "ace-builds/src-noconflict/theme-monokai";
import "ace-builds/src-noconflict/ext-language_tools";
import "ace-builds/src-noconflict/ext-searchbox";

class Editor extends Component {

    constructor(props) {
        super(props);

        this.state = {
            openFileModal: false,
            openFiles: ["macd_example", "crossover_example", "momentum_example", "rates_example"],
            tabValue: 1,
            loaded: false,
        };

        this.exchange = "GDAX"
        this.product = "DOGE-USD";
        this.code = props.code;
        this.fileDictionary = {};
        this.codeDirectory = "/home/satoshi/Projects/ctm-apollo/code/";

    }

    componentDidMount() {
        api.getCode(
            {
                fname: this.codeDirectory + "sample_code.py"
            }
        ).then(resp => {
            this.code = resp.data;
            console.log(this.code);
            this.setState({ loaded: true });
        });

    }


    handleChangeCode = (newValue) => {
        // console.log("change", newValue);
        this.code = newValue;
        // this.setState({ code: newValue });
    }

    handleChangeTab = (event, newValue) => {
        this.setState({
            tabValue: newValue,
        });
    }

    handleCloseModal = () => {
        this.setState({ openFileModal: false });
    }


    handleFileClick = (name) => {
        console.log(name);
        api.getCode(
            {
                fname: name,
            }
        ).then(resp => {
            console.log(resp.data)
            // this.code = resp.data;
            // this.handleChangeCode(resp.data);
        });
    }

    openCodeModal = () => {
        api.getFiles({ directory: this.codeDirectory }).then(resp => {
            this.fileDictionary = resp.data;
        });
        this.setState({ openFileModal: true });
    }


    render() {

        const {
            classes,
            handleSendCode,
            base,
            quote,
            // quoteValue,
        } = this.props;

        const {
            openFileModal,
            openFiles,
            tabValue,
            loaded,
        } = this.state;

        return (
            <div>
                <ListItem
                    header
                    label="Code Editor"
                    iconLeft={Code}
                    buttons={[
                        <Button plain onClick={this.openCodeModal}>
                            Open
                        </Button>,
                        <Button plain onClick={this.openCodeModal}>
                            Save
                        </Button>,
                        <Button plain onClick={this.openCodeModal}>
                            Close
                        </Button>
                    ]}
                />
                {loaded &&
                    <div>
                        <CustomizedTabs
                            tabs={openFiles}
                            // fontSize={fontSize}
                            value={tabValue}
                            handleChange={this.handleChangeTab}
                        />
                        {tabValue === 1 &&
                            <AceEditor
                                ref="aceEditor"
                                width="100%"
                                height="69vh"
                                mode="python"
                                theme="monokai"
                                fontSize={"1.2vh"}
                                onChange={this.handleChangeCode}
                                setOptions={{
                                    enableBasicAutocompletion: [autoComplete],
                                    //enableBasicAutocompletion: true,
                                    enableLiveAutocompletion: true,
                                    enableSnippets: true,
                                }}
                                // markers={this.errorMarkers}
                                name="UNIQUE_ID_OF_DIV"
                                editorProps={{ $blockScrolling: true }}
                                defaultValue={this.code}
                            />
                        }
                        <div className={classes.buttons}>
                            <Button secondary onClick={handleSendCode}>
                                RUN CODE
                            </Button>
                        </div>
                    </div>
                }
                <ListItem
                    header
                    borderTop
                    label="Code Output"
                    imgLeft={iconsUrl + 'output-icon.png'}
                />

                <FileModal
                    open={openFileModal}
                    onClose={this.handleCloseModal}
                    files={this.fileDictionary}
                    onFileClick={this.handleFileClick}
                />
            </div>
        );
    }
}

Editor.propTypes = {
    classes: PropTypes.object.isRequired,
    name: PropTypes.string,
    routes: PropTypes.any,
};

export default withStyles(editorStyle)(Editor);
