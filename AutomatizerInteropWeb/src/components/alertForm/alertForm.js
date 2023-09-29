import React from 'react';
import './alertForm.scss';
import { Popup } from 'devextreme-react/popup';
import { Button } from 'devextreme-react';
import { LoadIndicator } from 'devextreme-react/load-indicator';


export default class extends React.Component {


    constructor(props) {
        super(props);
        this.state={
            loadIndicatorVisible: false,
            buttonText: this.props.confirmBtnText
        };
    }


  

    handleClick() {
        this.setState(
          {
            loadIndicatorVisible: true,
            buttonText: this.props.confirmBtnText2
          }
        );

        this.props.onConfirm();
      }

    render(){
        if (this.props.data === null) {
            if (this.state.loadIndicatorVisible) {
                this.setState({
                    loadIndicatorVisible: false,
                    buttonText: this.props.confirmBtnText
                });
            }

            return null;        
          }
          return (
            <Popup
                visible={true}
                onHiding={this.props.onCancel}
                dragEnabled={false}
                closeOnOutsideClick={false}
                showTitle={true}
                showCloseButton={false}
                title={this.props.title}
                width={500}
                height={240}>
            <h5 className="popup-tittle">{this.props.question}</h5>
            <p className="text-muted lead popup-message">
                {this.props.message}
            </p>
            <div className="popup-buttons">
               <div  className="popup-buttons-container">
                   <div >
                        <Button
                        width={140}
                        text="Cancelar"
                        type="danger"
                        stylingMode="text"
                        disabled={this.state.loadIndicatorVisible}
                        onClick={this.props.onCancel}
                        >
                            
                        </Button>
           	        </div>    
                    <div>
                        <Button
                        width={160}
                        text={this.props.confirmBtnText}
                        type="success"
                        stylingMode="contained"
                        onClick={()=>this.handleClick()}
                        >
                            <LoadIndicator className="button-indicator" visible={this.state.loadIndicatorVisible} />    
                            <span className="dx-button-text">{this.state.buttonText}</span>
                        </Button>
                    </div>    
               </div>
           </div>
           
        </Popup>
            
          );
    }
}