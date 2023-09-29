import React from 'react';
import './customLoadIndicator.scss';
import { LoadIndicator } from 'devextreme-react/load-indicator';



export default class extends React.Component {
  render() {
    return (
      <React.Fragment>

        <h2 className={'content-block'}>{this.props.title}</h2>
        <div className={'content-block'}>
          <div className={'dx-card responsive-paddings'}>
            <LoadIndicator className="custom-indicator" />
            <span className="dx-button-text">{this.props.message}</span>
          </div>
        </div>
      </React.Fragment>
    )
  }
}