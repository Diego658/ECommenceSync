import React from 'react'
import './ImageGallery.scss'

export class ImageGallery extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      selectedItem: null,
      items: []
    }
  }



  changeSelected = (item) => {

    this.setState({ selectedItem: item });
  };

  render() {
    let galleryItems = [];
    return (
      <div className="gallery-container">
        {this.state.items.map(this.props.children, child => {
          galleryItems.push(child)
        })}
        <GalleryItems selectedItem={this.state.selectedItem} items={galleryItems} changeItem={(item) => this.setState({ selectedItem: item })} />
      </div>
    );
  }


}

const GalleryItems = ({ items, selectedItem, changeItem }) => {

  return (
    <>
      {items.map(item => {
        return <div key={item} className={item === selectedItem ? 'item item-selected' : 'item'} onClick={() => changeItem(item)} style={{ backgroundImage: `url(${item.url})` }} ></div>
      })}
    </>
  )
}